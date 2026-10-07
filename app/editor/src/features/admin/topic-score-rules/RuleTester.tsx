import React from 'react';
import { useLookup } from 'store/hooks';
import { useTopicScores } from 'store/hooks/admin';
import {
  Button,
  Checkbox,
  Col,
  type ITopicScoreRuleModel,
  OptionItem,
  Row,
  Select,
  Show,
  Text,
} from 'tno-core';

import {
  type ITopicScoreSourceModel,
  type ITopicScoreTestRequestModel,
  type ITopicScoreTestResultModel,
} from './interfaces';

export interface IRuleTesterProps {
  /** Sources that use topics. */
  sources: ITopicScoreSourceModel[];
  /** The source selected in the rules pane, used as the default. */
  selectedSourceId?: number;
  /** The selected source's rules, to describe each evaluation. */
  rules: ITopicScoreRuleModel[];
}

/**
 * Tests the rules against a saved story or entered values, showing the matched rule and score,
 * the fall back to the source default, and for every other rule the first condition that failed.
 * @param param0 Component properties.
 * @returns Component.
 */
export const RuleTester: React.FC<IRuleTesterProps> = ({ sources, selectedSourceId, rules }) => {
  const { test } = useTopicScores();
  const [{ series }] = useLookup();
  const [contentId, setContentId] = React.useState('');
  const [values, setValues] = React.useState<ITopicScoreTestRequestModel>({
    hasImage: false,
    characterCount: 0,
  });
  const [publishedOn, setPublishedOn] = React.useState('');
  const [result, setResult] = React.useState<ITopicScoreTestResultModel>();

  const sourceId = values.sourceId ?? selectedSourceId;
  const sourceOptions = sources.map((s) => new OptionItem(s.name, s.id));
  const seriesOptions = [
    new OptionItem('None', ''),
    ...series
      .filter((s) => !s.sourceId || s.sourceId === sourceId)
      .map((s) => new OptionItem(s.name, s.id)),
  ];

  const handleTest = async () => {
    try {
      const id = Number(contentId);
      const request: ITopicScoreTestRequestModel =
        contentId.trim() && Number.isInteger(id)
          ? { contentId: id, hasImage: false, characterCount: 0 }
          : {
              ...values,
              sourceId,
              publishedOn: publishedOn ? new Date(publishedOn).toISOString() : undefined,
            };
      setResult(await test(request));
    } catch {
      // Errors are handled globally.
    }
  };

  const ruleNumber = (ruleId?: number) => {
    const index = rules.findIndex((r) => r.id === ruleId);
    return index >= 0 ? `#${index + 1}` : `rule ${ruleId}`;
  };

  return (
    <Col className="rule-tester" gap="0.5rem">
      <h2>Rule tester</h2>
      <Row gap="0.5rem" alignItems="flex-end">
        <Text
          name="testContentId"
          label="Content ID"
          width="12ch"
          value={contentId}
          onChange={(e) => setContentId(e.target.value)}
        />
        <span className="hint">or enter values:</span>
      </Row>
      <Row gap="0.5rem" alignItems="flex-end" className={contentId.trim() ? 'disabled' : ''}>
        <Select
          name="testSourceId"
          label="Source"
          width="24ch"
          options={sourceOptions}
          value={sourceOptions.find((o) => o.value === sourceId)}
          onChange={(o) =>
            setValues({
              ...values,
              sourceId: (o as OptionItem)?.value as number,
              seriesId: undefined,
            })
          }
        />
        <Select
          name="testSeriesId"
          label="Series"
          width="20ch"
          options={seriesOptions}
          value={seriesOptions.find((o) => o.value === (values.seriesId ?? ''))}
          onChange={(o) => {
            const value = (o as OptionItem)?.value;
            setValues({ ...values, seriesId: value === '' ? undefined : (value as number) });
          }}
        />
        <Text
          name="testSection"
          label="Section"
          width="14ch"
          value={values.section ?? ''}
          onChange={(e) => setValues({ ...values, section: e.target.value })}
        />
        <Text
          name="testPage"
          label="Page"
          width="8ch"
          value={values.page ?? ''}
          onChange={(e) => setValues({ ...values, page: e.target.value })}
        />
        <Text
          name="testPublishedOn"
          label="Published"
          type="datetime-local"
          width="24ch"
          value={publishedOn}
          onChange={(e) => setPublishedOn(e.target.value)}
        />
        <Text
          name="testCharacterCount"
          label="Characters"
          type="number"
          min={0}
          width="12ch"
          value={values.characterCount}
          onChange={(e) => setValues({ ...values, characterCount: Number(e.target.value) || 0 })}
        />
        <Checkbox
          name="testHasImage"
          label="Has image"
          checked={values.hasImage}
          onChange={(e) => setValues({ ...values, hasImage: e.target.checked })}
        />
        <Button onClick={handleTest} disabled={!contentId.trim() && !sourceId}>
          Test
        </Button>
      </Row>
      <Show visible={!!result}>
        {result && (
          <Col className="test-result" gap="0.25rem">
            <Show visible={!result.isEligible}>
              <p className="warning">
                This content is not scored: neither its source nor its series uses topics, or it is
                Image content.
              </p>
            </Show>
            <p>
              <b>Score {result.score}</b>{' '}
              {result.ruleId
                ? `from ${ruleNumber(result.ruleId)}.`
                : result.isSourceDefault
                ? 'from the source default, because no rule matched.'
                : 'because no rule matched and the source has no default.'}
            </p>
            <ul className="evaluations">
              {result.evaluations.map((e) => (
                <li key={e.ruleId} className={e.isMatch ? 'match' : ''}>
                  {ruleNumber(e.ruleId)} (score {e.score}):{' '}
                  {e.isMatch
                    ? 'matched'
                    : `${e.failedCondition ? `${e.failedCondition} — ` : ''}${e.reason ?? ''}`}
                </li>
              ))}
            </ul>
          </Col>
        )}
      </Show>
    </Col>
  );
};
