import moment from 'moment';
import React from 'react';
import { toast } from 'react-toastify';
import { useContentAnalysisAdmin } from 'store/hooks/admin';
import {
  Button,
  ButtonVariant,
  Col,
  Modal,
  OptionItem,
  Row,
  Select,
  Show,
  Text,
  useModal,
} from 'tno-core';

import {
  type AnalysisBackfillDateFieldName,
  type AnalysisBackfillModeName,
  type IAnalysisBackfillModel,
  type IAnalysisBackfillPreviewModel,
  type IAnalysisBackfillRequestModel,
} from './interfaces';

const dateFieldOptions = [
  new OptionItem('Publication date', 'PublishedOn'),
  new OptionItem('Creation date', 'CreatedOn'),
];

const modeOptions = [
  new OptionItem('Missing or out-of-date analysis only', 'MissingOrStale'),
  new OptionItem('Everything (force reanalysis)', 'Force'),
];

const statusNames: Record<IAnalysisBackfillModel['status'], string> = {
  Submitted: 'Waiting to start',
  InProgress: 'Sending stories',
  Completed: 'All stories sent',
  Cancelled: 'Cancelled',
  Failed: 'Failed',
};

const isActive = (backfill: IAnalysisBackfillModel) =>
  backfill.status === 'Submitted' || backfill.status === 'InProgress';

/**
 * Analyze existing stories in a date range. A backfill is a work order: the Event Handler sends the
 * stories in the range to Content-Analysis, which analyzes them apart from new stories and with a
 * limited share of the model's throughput; reports never wait for it.
 * @returns Component.
 */
export const BackfillPanel: React.FC = () => {
  const api = useContentAnalysisAdmin();
  const { toggle, isShowing } = useModal();
  const timeZone = Intl.DateTimeFormat().resolvedOptions().timeZone;
  const [startOn, setStartOn] = React.useState(moment().subtract(30, 'days').format('YYYY-MM-DD'));
  const [endOn, setEndOn] = React.useState(moment().format('YYYY-MM-DD'));
  const [dateField, setDateField] = React.useState<AnalysisBackfillDateFieldName>('PublishedOn');
  const [mode, setMode] = React.useState<AnalysisBackfillModeName>('MissingOrStale');
  const [preview, setPreview] = React.useState<IAnalysisBackfillPreviewModel>();
  const [backfills, setBackfills] = React.useState<IAnalysisBackfillModel[]>([]);

  // Local days in the viewer's time zone, sent as the UTC instants they begin.
  const request: IAnalysisBackfillRequestModel = {
    startOn: moment(startOn).startOf('day').toISOString(),
    endOn: moment(endOn).startOf('day').toISOString(),
    timeZone,
    dateField,
    mode,
  };
  const isValid = !!startOn && !!endOn && moment(endOn).isAfter(moment(startOn));

  const refresh = React.useCallback(async () => {
    try {
      setBackfills(await api.findBackfills());
    } catch {}
  }, [api]);

  React.useEffect(() => {
    refresh();
  }, [refresh]);

  // Refresh progress while a backfill is running.
  const hasActive = backfills.some(isActive);
  React.useEffect(() => {
    if (!hasActive) return;
    const timer = setInterval(refresh, 5000);
    return () => clearInterval(timer);
  }, [hasActive, refresh]);

  const handlePreview = async () => {
    try {
      setPreview(await api.previewBackfill(request));
    } catch {}
  };

  const handleStart = async () => {
    try {
      await api.startBackfill(request);
      toast.success('Backfill started.');
      setPreview(undefined);
      await refresh();
    } catch {}
  };

  return (
    <Col className="panel" gap="0.5rem">
      <h2>Backfill</h2>
      <p className="hint">
        Analyze stories that already exist. The range starts on the first day and ends before the
        second, in your time zone ({timeZone}).
      </p>
      <Row gap="1rem" alignItems="flex-end" className="field-row">
        <Text
          name="backfillStartOn"
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
          name="backfillEndOn"
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
          name="backfillDateField"
          label="Date"
          width="22ch"
          isClearable={false}
          options={dateFieldOptions}
          value={dateFieldOptions.find((o) => o.value === dateField)}
          onChange={(o) => {
            setDateField(((o as OptionItem)?.value as AnalysisBackfillDateFieldName) ?? dateField);
            setPreview(undefined);
          }}
        />
        <Select
          name="backfillMode"
          label="Stories"
          width="38ch"
          isClearable={false}
          options={modeOptions}
          value={modeOptions.find((o) => o.value === mode)}
          onChange={(o) => {
            setMode(((o as OptionItem)?.value as AnalysisBackfillModeName) ?? mode);
            setPreview(undefined);
          }}
        />
        <Button variant={ButtonVariant.secondary} onClick={handlePreview} disabled={!isValid}>
          Preview
        </Button>
        <Button onClick={toggle} disabled={!isValid || !preview}>
          Start
        </Button>
      </Row>
      <Show visible={!!preview}>
        {preview && (
          <p>
            {preview.matching.toLocaleString()} stories in range
            {mode === 'MissingOrStale'
              ? `, ${preview.alreadyCurrent.toLocaleString()} already analyzed`
              : ''}
            . {preview.excluded.toLocaleString()} excluded by media type or source.
            {dateField === 'PublishedOn'
              ? ` ${preview.missingPublicationDate.toLocaleString()} created in the range have no publication date and are not included.`
              : ''}
          </p>
        )}
      </Show>
      <div className="backfills">
        {backfills.map((b) => (
          <Row key={b.id} gap="1rem" alignItems="center" className="backfill">
            <b>#{b.id}</b>
            <span>
              {moment(b.startOn).format('YYYY-MM-DD')} to {moment(b.endOn).format('YYYY-MM-DD')} (
              {b.dateField === 'PublishedOn' ? 'published' : 'created'}, {b.timeZone})
            </span>
            <span>{statusNames[b.status] ?? b.status}</span>
            <span>
              {b.scheduled.toLocaleString()} of {b.total.toLocaleString()} sent ·{' '}
              {b.alreadyCurrent.toLocaleString()} already current · {b.failed.toLocaleString()}{' '}
              failed
            </span>
            {b.error && <span className="error">{b.error}</span>}
            <Show visible={isActive(b)}>
              <Button
                variant={ButtonVariant.link}
                onClick={() => api.cancelBackfill(b.id).then(refresh)}
              >
                Cancel
              </Button>
            </Show>
            <Show visible={b.status === 'Cancelled' || b.status === 'Failed'}>
              <Button
                variant={ButtonVariant.link}
                onClick={() => api.resumeBackfill(b.id).then(refresh)}
              >
                Resume
              </Button>
            </Show>
          </Row>
        ))}
      </div>
      <Modal
        headerText="Start backfill"
        body={`Analyze ${
          preview?.matching.toLocaleString() ?? ''
        } stories in this range? Backfill never delays new stories.`}
        isShowing={isShowing}
        hide={toggle}
        type="default"
        confirmText="Yes, start"
        onConfirm={async () => {
          try {
            await handleStart();
          } finally {
            toggle();
          }
        }}
      />
    </Col>
  );
};
