import { FormPage } from 'components/formpage';
import React from 'react';
import { toast } from 'react-toastify';
import { useTopicScores } from 'store/hooks/admin';
import { type ITopicScoreRuleModel } from 'tno-core';

import { BulkRescore } from './BulkRescore';
import { defaultTopicScoreRule } from './constants';
import { type ITopicScoreRuleForm, type ITopicScoreSourceModel } from './interfaces';
import { RuleDrawer } from './RuleDrawer';
import { RulesPane } from './RulesPane';
import { RuleTester } from './RuleTester';
import { SourcesPane } from './SourcesPane';
import * as styled from './styled';
import { toForm } from './utils';

/**
 * Topic score administration: sources that use topics, each source's ordered rules, a rule
 * tester, and bulk rescore.
 * @returns Component.
 */
const TopicScoreAdmin: React.FC = () => {
  const api = useTopicScores();
  const [sources, setSources] = React.useState<ITopicScoreSourceModel[]>([]);
  const [source, setSource] = React.useState<ITopicScoreSourceModel>();
  const [rules, setRules] = React.useState<ITopicScoreRuleModel[]>([]);
  const [sections, setSections] = React.useState<string[]>([]);
  const [editing, setEditing] = React.useState<ITopicScoreRuleForm>();

  React.useEffect(() => {
    api
      .findSources()
      .then((sources) => {
        setSources(sources);
        setSource((current) => current ?? sources[0]);
      })
      .catch(() => {});
  }, [api]);

  React.useEffect(() => {
    if (!source) return;
    setEditing(undefined);
    api
      .findRules(source.id)
      .then(setRules)
      .catch(() => {});
    api
      .findSections(source.id)
      .then(setSections)
      .catch(() => setSections([]));
  }, [api, source]);

  /** Keep the source list's rule count in step with the rules pane. */
  const storeRules = (sourceId: number, rules: ITopicScoreRuleModel[]) => {
    setRules(rules);
    setSources((sources) =>
      sources.map((s) => (s.id === sourceId ? { ...s, ruleCount: rules.length } : s)),
    );
  };

  const handleDefaultScoreChange = async (model: ITopicScoreSourceModel) => {
    try {
      const result = await api.updateSourceDefaultScore(model);
      setSources((sources) => sources.map((s) => (s.id === result.id ? result : s)));
      setSource((current) => (current?.id === result.id ? result : current));
      toast.success(`${result.name} default score saved.`);
    } catch {}
  };

  const handleSave = async (model: ITopicScoreRuleModel) => {
    if (!source) return;
    const result = model.id ? await api.updateRule(model) : await api.addRule(model);
    storeRules(
      source.id,
      model.id ? rules.map((r) => (r.id === result.id ? result : r)) : [...rules, result],
    );
    setEditing(undefined);
    toast.success('Rule saved.');
  };

  const handleDelete = async (model: ITopicScoreRuleModel) => {
    if (!source) return;
    await api.deleteRule(model);
    storeRules(
      source.id,
      rules.filter((r) => r.id !== model.id),
    );
    setEditing(undefined);
    toast.success('Rule deleted.');
  };

  const handleReorder = async (reordered: ITopicScoreRuleModel[]) => {
    if (!source) return;
    const previous = rules;
    setRules(reordered);
    try {
      setRules(
        await api.reorderRules(
          source.id,
          reordered.map((r) => r.id),
        ),
      );
    } catch {
      setRules(previous);
    }
  };

  return (
    <styled.TopicScoreAdmin>
      <FormPage>
        <p className="page-description">
          A topic score is a story's prominence (front page, image, lead time slot). Event of the
          Day sums scores by topic. Each source's rules are checked in order and the first match
          sets the score.
        </p>
        <div className="panes">
          <SourcesPane
            sources={sources}
            selectedId={source?.id}
            onSelect={setSource}
            onDefaultScoreChange={handleDefaultScoreChange}
          />
          {source && (
            <RulesPane
              source={source}
              rules={rules}
              onAdd={() => setEditing(defaultTopicScoreRule(source.id))}
              onEdit={(rule) => setEditing(toForm(rule))}
              onReorder={handleReorder}
            />
          )}
        </div>
        <RuleTester sources={sources} selectedSourceId={source?.id} rules={rules} />
        <BulkRescore sources={sources} />
      </FormPage>
      {source && editing && (
        <RuleDrawer
          source={source}
          rule={editing}
          sections={sections}
          onSave={handleSave}
          onDelete={handleDelete}
          onClose={() => setEditing(undefined)}
        />
      )}
    </styled.TopicScoreAdmin>
  );
};

export default TopicScoreAdmin;
