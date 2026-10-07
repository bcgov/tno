import moment from 'moment';
import React from 'react';
import { toast } from 'react-toastify';
import { useTopicScores } from 'store/hooks/admin';
import { Button, Col, Modal, OptionItem, Row, Select, Show, Text, useModal } from 'tno-core';

import {
  BackgroundJobStatusName,
  type ITopicRescoreJobModel,
  type ITopicRescorePreviewModel,
  type ITopicRescoreRequestModel,
  type ITopicScoreSourceModel,
} from './interfaces';

export interface IBulkRescoreProps {
  /** Sources that use topics. */
  sources: ITopicScoreSourceModel[];
}

const isActive = (job?: ITopicRescoreJobModel) =>
  job?.status === BackgroundJobStatusName.Pending ||
  job?.status === BackgroundJobStatusName.Running;

/**
 * Recalculates the calculated scores of stories in a date range after rules change. Overridden
 * scores are never touched. Runs as a background job that re-indexes the stories that changed.
 * @param param0 Component properties.
 * @returns Component.
 */
export const BulkRescore: React.FC<IBulkRescoreProps> = ({ sources }) => {
  const { previewRescore, rescore, findRescoreJobs, findRescoreJob } = useTopicScores();
  const { toggle, isShowing } = useModal();
  const [startOn, setStartOn] = React.useState(moment().subtract(1, 'day').format('YYYY-MM-DD'));
  const [endOn, setEndOn] = React.useState(moment().add(1, 'day').format('YYYY-MM-DD'));
  const [sourceIds, setSourceIds] = React.useState<number[]>([]);
  const [preview, setPreview] = React.useState<ITopicRescorePreviewModel>();
  const [jobs, setJobs] = React.useState<ITopicRescoreJobModel[]>([]);

  const sourceOptions = sources.map((s) => new OptionItem(s.name, s.id));
  // Dates are the user's local days; the API receives the UTC instants they begin.
  const request: ITopicRescoreRequestModel = {
    startOn: moment(startOn).startOf('day').toISOString(),
    endOn: moment(endOn).startOf('day').toISOString(),
    sourceIds,
  };
  const isValid = !!startOn && !!endOn && moment(endOn).isAfter(moment(startOn));

  React.useEffect(() => {
    findRescoreJobs()
      .then(setJobs)
      .catch(() => {});
  }, [findRescoreJobs]);

  // Poll the running job until it finishes.
  const activeJob = jobs.find(isActive);
  React.useEffect(() => {
    if (!activeJob) return;
    const timer = setInterval(async () => {
      try {
        const job = await findRescoreJob(activeJob.id);
        setJobs((jobs) => jobs.map((j) => (j.id === job.id ? job : j)));
        if (!isActive(job) && job.status === BackgroundJobStatusName.Completed)
          toast.success(`Rescore finished: ${job.changed} of ${job.total} stories changed.`);
      } catch {}
    }, 3000);
    return () => clearInterval(timer);
  }, [activeJob, findRescoreJob]);

  const handlePreview = async () => {
    try {
      setPreview(await previewRescore(request));
    } catch {}
  };

  const handleRun = async () => {
    try {
      const job = await rescore(request);
      setJobs((jobs) => [job, ...jobs]);
      setPreview(undefined);
    } catch {}
  };

  return (
    <Col className="bulk-rescore" gap="0.5rem">
      <h2>Bulk rescore</h2>
      <p className="hint">
        Recalculate the scores of stories published in a date range, for example after changing
        rules. Scores set by an editor or by ingest are left alone.
      </p>
      <Row gap="0.5rem" alignItems="flex-end">
        <Text
          name="rescoreStartOn"
          label="From"
          type="date"
          width="18ch"
          value={startOn}
          onChange={(e) => {
            setStartOn(e.target.value);
            setPreview(undefined);
          }}
        />
        <Text
          name="rescoreEndOn"
          label="Before"
          type="date"
          width="18ch"
          value={endOn}
          onChange={(e) => {
            setEndOn(e.target.value);
            setPreview(undefined);
          }}
        />
        <Select
          name="rescoreSourceIds"
          label="Sources (all when empty)"
          width="40ch"
          isMulti
          options={sourceOptions}
          value={sourceOptions.filter((o) => sourceIds.includes(o.value as number))}
          onChange={(o) => {
            setSourceIds(((o as OptionItem[]) ?? []).map((i) => i.value as number));
            setPreview(undefined);
          }}
        />
        <Button onClick={handlePreview} disabled={!isValid}>
          Preview
        </Button>
        <Button onClick={toggle} disabled={!isValid || !!activeJob}>
          Rescore
        </Button>
      </Row>
      <Show visible={!!preview}>
        <p>
          {preview?.changed.toLocaleString()} of {preview?.total.toLocaleString()} stories would get
          a new score.
        </p>
      </Show>
      <Show visible={!!jobs.length}>
        <div className="jobs">
          {jobs.map((job) => (
            <div key={job.id} className="job">
              <b>#{job.id}</b> {moment(job.startOn).format('YYYY-MM-DD')} to{' '}
              {moment(job.endOn).format('YYYY-MM-DD')} — {job.status}
              {job.total > 0 && ` · ${job.processed}/${job.total} processed`}
              {` · ${job.changed} changed`}
              {job.failed > 0 && ` · ${job.failed} failed`}
              {job.error && <span className="error"> · {job.error}</span>}
            </div>
          ))}
        </div>
      </Show>
      <Modal
        headerText="Rescore stories"
        body="Recalculate the calculated scores of the stories in this range and re-index the ones that change?"
        isShowing={isShowing}
        hide={toggle}
        type="default"
        confirmText="Yes, rescore"
        onConfirm={async () => {
          try {
            await handleRun();
          } finally {
            toggle();
          }
        }}
      />
    </Col>
  );
};
