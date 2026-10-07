import React from 'react';
import { useHistoryRetention } from 'store/hooks/admin';
import { Button, ButtonVariant, Col, type ISettingModel, Row, Show } from 'tno-core';

import { type IHistoryPurgeModel, type IHistoryPurgePreviewModel } from './interfaces';

export interface IHistoryRetentionPanelProps {
  /** All settings, used to show the configured retention. */
  settings: ISettingModel[];
}

const describeRetention = (settings: ISettingModel[], name: string) => {
  const setting = settings.find((s) => s.name === name);
  if (!setting) return 'not configured (default applies)';
  const days = Number(setting.value);
  if (!setting.isEnabled || !Number.isInteger(days) || days <= 0) return 'purge disabled';
  return `${days} days`;
};

const PurgeCounts: React.FC<{ label: string; result: IHistoryPurgeModel }> = ({
  label,
  result,
}) => {
  const tables = Object.entries(result.tables);
  return (
    <Col className="purge-counts">
      <b>{label}</b>
      <Show visible={result.retentionDays <= 0}>
        <span>Purge disabled.</span>
      </Show>
      <Show visible={result.retentionDays > 0 && !tables.length}>
        <span>Nothing to purge.</span>
      </Show>
      {tables.map(([table, count]) => (
        <span key={table}>
          {table}: {count.toLocaleString()}
        </span>
      ))}
    </Col>
  );
};

/**
 * Shows the configured history retention and what the daily purge would delete now.
 * @param param0 Component properties.
 * @returns Component.
 */
export const HistoryRetentionPanel: React.FC<IHistoryRetentionPanelProps> = ({ settings }) => {
  const { previewHistoryPurge } = useHistoryRetention();
  const [preview, setPreview] = React.useState<IHistoryPurgePreviewModel>();
  const [isLoading, setIsLoading] = React.useState(false);

  const handlePreview = async () => {
    try {
      setIsLoading(true);
      setPreview(await previewHistoryPurge());
    } catch {
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <Col className="history-retention">
      <Row gap="1rem" alignItems="center">
        <Col flex="1 1 0">
          <b>History retention</b>
          <span>
            Reports: {describeRetention(settings, 'ReportRetentionDays')}. Notifications:{' '}
            {describeRetention(settings, 'NotificationRetentionDays')}. Change the
            ReportRetentionDays and NotificationRetentionDays settings; 0 disables a purge. The
            purge runs daily through the "Purge History" event schedule.
          </span>
        </Col>
        <Button variant={ButtonVariant.secondary} onClick={handlePreview} disabled={isLoading}>
          Preview purge
        </Button>
      </Row>
      <Show visible={!!preview}>
        {preview && (
          <Row gap="2rem">
            <PurgeCounts label="Report history" result={preview.reports} />
            <PurgeCounts label="Notification history" result={preview.notifications} />
          </Row>
        )}
      </Show>
    </Col>
  );
};
