import { Button } from 'components/button';
import { formatDate, formatTime } from 'features/utils';
import { FaX } from 'react-icons/fa6';
import { FaEye, FaRegClock } from 'react-icons/fa6';
import { useReportsStore } from 'store/slices';
import { Col, Row } from 'tno-core';

import { useReportEditContext } from '../ReportEditContext';
import * as styled from './styled';
export const ReportHistoryView = () => {
  const [{ reportView }, { storeReportView }] = useReportsStore();
  const { activeInstance } = useReportEditContext();

  if (!reportView || !activeInstance) return null;

  return (
    <styled.ReportHistoryView>
      <div>
        <Row className="report-history-headline" alignItems="first baseline" gap="0.5em">
          <FaEye size={18} />
          <h1>Previous Report</h1>
          <h2>{formatDate(activeInstance.createdOn)}</h2>
          <FaRegClock size={18} />
          <h2>{formatTime(activeInstance.createdOn)}</h2>
          <Button
            variant="info"
            onClick={() => {
              storeReportView(undefined);
            }}
          >
            <FaX />
          </Button>
        </Row>
      </div>
      <Col className="preview-report">
        <div
          className="preview-subject"
          dangerouslySetInnerHTML={{ __html: reportView?.subject ?? '' }}
        ></div>
        <div
          className="preview-body"
          dangerouslySetInnerHTML={{ __html: reportView?.body ?? '' }}
        ></div>
      </Col>
    </styled.ReportHistoryView>
  );
};
