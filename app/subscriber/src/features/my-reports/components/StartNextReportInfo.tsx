import { formatDate } from 'features/utils';
import { FaInfoCircle } from 'react-icons/fa';
import { getReportKind, ReportKindName, ReportStatusName, Row, Show } from 'tno-core';

import { useReportEditContext } from '../edit/ReportEditContext';

/**
 * Provides a simple explanation for why a report is readonly and how to start the next report.
 * @returns Component
 */
export const StartNextReportInfo = () => {
  const { values } = useReportEditContext();

  const instance = values.instances.length ? values.instances[0] : undefined;
  const showReadonly =
    instance &&
    [
      ReportStatusName.Submitted,
      ReportStatusName.Accepted,
      ReportStatusName.Cancelled,
      ReportStatusName.Completed,
      ReportStatusName.Failed,
    ].includes(instance.status);

  return (
    <>
      <Show visible={!!instance && !!instance.sentOn}>
        <div className="report-info">
          <Row alignItems="center" gap="1rem">
            <FaInfoCircle />
            <p>
              This report was sent to subscribers on{' '}
              {`${formatDate(instance?.sentOn?.toLocaleString(), true)}`}.{' '}
              {showReadonly && (
                <span>
                  This report is <strong>readonly</strong> until the next report is started.
                </span>
              )}
            </p>
          </Row>
        </div>
      </Show>
      <Show
        visible={
          [ReportKindName.Auto, ReportKindName.AutoSend].includes(getReportKind(values)) &&
          values.settings.content.clearOnStartNewReport &&
          !instance?.sentOn
        }
      >
        <div className="report-info">
          This is an Auto Report and has been configured to clear all content when generating the
          next report. Any changes you make will be lost if the next scheduled run is executed
          before you send it to subscribers.
        </div>
      </Show>
    </>
  );
};
