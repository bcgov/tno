import { FormPage } from 'components/formpage';
import React from 'react';
import { Link } from 'react-router-dom';
import { toast } from 'react-toastify';
import { useLookup } from 'store/hooks';
import { useContentAnalysisAdmin } from 'store/hooks/admin';
import { Button, ButtonVariant, Col, OptionItem, Row, Select, Show } from 'tno-core';

import { type IAnalysisJobModel } from '../../content/form/interfaces';
import { BackfillPanel } from './BackfillPanel';
import { type IContentAnalysisSettingsModel } from './interfaces';
import * as styled from './styled';

const statusOptions = [new OptionItem('Failed', 'Failed'), new OptionItem('Skipped', 'Skipped')];

/**
 * Content-Analysis administration: the feature switch and LLM, excluded media types and sources,
 * the work queue, failed jobs and replay, and backfill. The processes it runs are configured on
 * the Content-Analysis service.
 * @returns Component.
 */
const ContentAnalysisAdmin: React.FC = () => {
  const api = useContentAnalysisAdmin();
  const [{ mediaTypes, sources, llms }, { getLLMs }] = useLookup();
  const [settings, setSettings] = React.useState<IContentAnalysisSettingsModel>();
  const [counts, setCounts] = React.useState<Record<string, number>>({});
  const [status, setStatus] = React.useState('Failed');
  const [jobs, setJobs] = React.useState<IAnalysisJobModel[]>([]);

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
      setCounts((await api.getQueue()).counts);
      setJobs(await api.findJobs(status));
    } catch {}
  }, [api, status]);

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

  const handleReplay = async (job: IAnalysisJobModel) => {
    try {
      await api.replayJob(job.id);
      toast.success(`Analysis of story ${job.contentId} queued again.`);
      await refreshQueue();
    } catch {}
  };

  const countKeys = Object.keys(counts).sort();

  return (
    <styled.ContentAnalysisAdmin>
      <FormPage>
        <p className="page-description">
          Content-Analysis reads each story after it is added or changed, stores what it finds, and
          fills empty summaries, tags, contributors, quotes, and topics — for the processes the
          Content-Analysis service is configured to run. It never changes a value a person or
          automation set.
        </p>

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
                    excludedMediaTypeIds: ((o as OptionItem[]) ?? []).map((i) => i.value as number),
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
                    excludedSourceIds: ((o as OptionItem[]) ?? []).map((i) => i.value as number),
                  })
                }
              />
              <p className="hint">
                Topic population is controlled on the <Link to="/admin/topics">Topics</Link> page.
              </p>
              <Row>
                <Button onClick={handleSave}>Save</Button>
              </Row>
            </Col>
          )}
        </Show>

        <Col className="panel" gap="0.5rem">
          <Row gap="1rem" alignItems="center">
            <h2>Queue</h2>
            <Button variant={ButtonVariant.secondary} onClick={refreshQueue}>
              Refresh
            </Button>
          </Row>
          <div className="counts">
            {countKeys.map((key) => (
              <span key={key}>
                {key.replace(':', ' · ')}: <b>{counts[key].toLocaleString()}</b>
              </span>
            ))}
          </div>
          <Row gap="1rem" alignItems="flex-end">
            <Select
              name="status"
              label="Jobs"
              width="20ch"
              isClearable={false}
              options={statusOptions}
              value={statusOptions.find((o) => o.value === status)}
              onChange={(o) => setStatus(((o as OptionItem)?.value as string) ?? 'Failed')}
            />
          </Row>
          <div className="jobs">
            <Show visible={!jobs.length}>
              <p>No {status.toLowerCase()} jobs.</p>
            </Show>
            {jobs.map((job) => (
              <Row key={job.id} gap="1rem" alignItems="center" className="job">
                <Link to={`/contents/${job.contentId}`}>Story {job.contentId}</Link>
                <span>{job.reason}</span>
                <span>{job.attempts} attempt(s)</span>
                <span className="error">{job.lastError}</span>
                <Button variant={ButtonVariant.link} onClick={() => handleReplay(job)}>
                  Replay
                </Button>
              </Row>
            ))}
          </div>
        </Col>

        <BackfillPanel />
      </FormPage>
    </styled.ContentAnalysisAdmin>
  );
};

export default ContentAnalysisAdmin;
