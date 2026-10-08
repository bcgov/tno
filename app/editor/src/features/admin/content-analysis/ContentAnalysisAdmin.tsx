import { FormPage } from 'components/formpage';
import React from 'react';
import { Link } from 'react-router-dom';
import { toast } from 'react-toastify';
import { useLookup } from 'store/hooks';
import { useContentAnalysisAdmin } from 'store/hooks/admin';
import { Button, ButtonVariant, Col, OptionItem, Row, Select, Show, Tab, Tabs } from 'tno-core';

import { BackfillPanel } from './BackfillPanel';
import {
  type IAnalysisFailureModel,
  type IAnalysisQueueModel,
  type IContentAnalysisSettingsModel,
} from './interfaces';
import * as styled from './styled';
import { TopicPopulationPanel } from './TopicPopulationPanel';

const topicNames: Record<string, string> = {
  analysis: 'New and changed stories',
  'analysis-retry': 'Retries',
  'analysis-backfill': 'Backfill',
};

type TabName = 'settings' | 'topics' | 'queue' | 'backfill';

/**
 * Content-Analysis administration, a tab each: the LLM and excluded media types and sources,
 * automatic topics, the requests waiting in Kafka with failed stories and replay, and backfill. The
 * processes it runs are configured on the Content-Analysis service.
 * @returns Component.
 */
const ContentAnalysisAdmin: React.FC = () => {
  const api = useContentAnalysisAdmin();
  const [{ mediaTypes, sources, llms }, { getLLMs }] = useLookup();
  const [settings, setSettings] = React.useState<IContentAnalysisSettingsModel>();
  const [queue, setQueue] = React.useState<IAnalysisQueueModel>({ lag: {}, failed: 0 });
  const [failures, setFailures] = React.useState<IAnalysisFailureModel[]>([]);
  const [active, setActive] = React.useState<TabName>('settings');

  React.useEffect(() => {
    api
      .getSettings()
      .then(setSettings)
      .catch(() => {});
    if (!llms.length) getLLMs().catch(() => {});
    // Load once.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [api]);

  const refreshQueue = React.useCallback(async () => {
    try {
      setQueue(await api.getQueue());
      setFailures(await api.findFailures());
    } catch {}
  }, [api]);

  React.useEffect(() => {
    refreshQueue();
  }, [refreshQueue]);

  const llmOptions = llms.filter((l) => !l.agentName).map((l) => new OptionItem(l.name, l.id));
  const mediaTypeOptions = mediaTypes.map((m) => new OptionItem(m.name, m.id));
  const sourceOptions = sources.map((s) => new OptionItem(s.name, s.id));

  const handleSave = async () => {
    if (!settings) return;
    try {
      setSettings(await api.updateSettings(settings));
      toast.success('Content-Analysis settings saved.');
    } catch {}
  };

  const handleReplay = async (failure: IAnalysisFailureModel) => {
    try {
      await api.replay(failure.contentId);
      toast.success(`Story ${failure.contentId} sent for analysis again.`);
    } catch {}
  };

  const topics = Object.keys(queue.lag).sort();

  return (
    <styled.ContentAnalysisAdmin>
      <FormPage>
        <p className="page-description">
          Content-Analysis reads each story after it is added or changed, stores what it finds, and
          fills empty summaries, tags, contributors, quotes, and topics — for the processes the
          Content-Analysis service is configured to run. It never changes a value a person or
          automation set.
        </p>

        <Tabs
          tabs={
            <>
              <Tab
                label="Settings"
                onClick={() => setActive('settings')}
                active={active === 'settings'}
              />
              <Tab
                label="Topics"
                onClick={() => setActive('topics')}
                active={active === 'topics'}
              />
              <Tab label="Queue" onClick={() => setActive('queue')} active={active === 'queue'} />
              <Tab
                label="Backfill"
                onClick={() => setActive('backfill')}
                active={active === 'backfill'}
              />
            </>
          }
        >
          <Show visible={active === 'settings'}>
            <Show visible={!!settings}>
              {settings && (
                <Col className="panel" gap="0.5rem">
                  <h2>Settings</h2>
                  <Row gap="1rem" alignItems="flex-end">
                    <Select
                      name="llmId"
                      label="LLM"
                      tooltip="A direct-model LLM with a context window and maximum output configured"
                      width="30ch"
                      options={llmOptions}
                      value={llmOptions.find((o) => o.value === settings.llmId)}
                      onChange={(o) =>
                        setSettings({ ...settings, llmId: (o as OptionItem)?.value as number })
                      }
                    />
                  </Row>
                  <Select
                    name="excludedMediaTypeIds"
                    label="Media types not analyzed"
                    isMulti
                    options={mediaTypeOptions}
                    value={mediaTypeOptions.filter((o) =>
                      settings.excludedMediaTypeIds.includes(o.value as number),
                    )}
                    onChange={(o) =>
                      setSettings({
                        ...settings,
                        excludedMediaTypeIds: ((o as OptionItem[]) ?? []).map(
                          (i) => i.value as number,
                        ),
                      })
                    }
                  />
                  <Select
                    name="excludedSourceIds"
                    label="Sources not analyzed"
                    isMulti
                    options={sourceOptions}
                    value={sourceOptions.filter((o) =>
                      settings.excludedSourceIds.includes(o.value as number),
                    )}
                    onChange={(o) =>
                      setSettings({
                        ...settings,
                        excludedSourceIds: ((o as OptionItem[]) ?? []).map(
                          (i) => i.value as number,
                        ),
                      })
                    }
                  />
                  <Row>
                    <Button onClick={handleSave}>Save</Button>
                  </Row>
                </Col>
              )}
            </Show>
          </Show>
          <Show visible={active === 'topics'}>
            <TopicPopulationPanel />
          </Show>
          <Show visible={active === 'queue'}>
            <Col className="panel" gap="0.5rem">
              <h2>Queue</h2>
              <p className="hint">
                Stories waiting in each Kafka topic for Content-Analysis, and the stories whose most
                recent analysis request failed.
              </p>
              <Row gap="1rem" alignItems="flex-end" className="field-row">
                <Button variant={ButtonVariant.secondary} onClick={refreshQueue}>
                  Refresh
                </Button>
              </Row>
              <div className="counts">
                {topics.map((topic) => (
                  <span key={topic}>
                    {topicNames[topic] ?? topic}: <b>{queue.lag[topic].toLocaleString()}</b> waiting
                  </span>
                ))}
                <span>
                  Failed: <b>{queue.failed.toLocaleString()}</b>
                </span>
              </div>
              <div className="failures">
                <Show visible={!failures.length}>
                  <p>No failed stories.</p>
                </Show>
                {failures.map((failure) => (
                  <Row key={failure.contentId} gap="1rem" alignItems="center" className="failure">
                    <Link to={`/contents/${failure.contentId}`}>
                      {failure.headline || `Story ${failure.contentId}`}
                    </Link>
                    <span>{failure.run.reason}</span>
                    <span>{failure.run.attempts} attempt(s)</span>
                    <span className="error">{failure.run.error}</span>
                    <Button variant={ButtonVariant.link} onClick={() => handleReplay(failure)}>
                      Replay
                    </Button>
                  </Row>
                ))}
              </div>
            </Col>
          </Show>
          <Show visible={active === 'backfill'}>
            <BackfillPanel />
          </Show>
        </Tabs>
      </FormPage>
    </styled.ContentAnalysisAdmin>
  );
};

export default ContentAnalysisAdmin;
