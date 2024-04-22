import { FaInfoCircle } from 'react-icons/fa';
import { formatDate, Row, Show } from 'tno-core';

import { useReportEditContext } from '../edit/ReportEditContext';

/**
 * Provides a simple explanation for why a report is readonly and how to start the next report.
 * @returns Component
 */
export const StartNextReportInfo = () => {
  const { values } = useReportEditContext();

  const instance = values.instances.length ? values.instances[0] : undefined;

  return (
    <Show visible={!!instance && !!instance.sentOn}>
      <div className="report-info">
        <Row alignItems="center" gap="1rem">
          <FaInfoCircle />
          <p>
            This report was sent to subscribers on{' '}
            {`${formatDate(instance?.sentOn?.toLocaleString(), 'YYYY-MM-DD hh:mm:ss a')}`}. This
            report is <strong>readonly</strong> until the next report is started.
          </p>
        </Row>
      </div>
    </Show>
  );
};
