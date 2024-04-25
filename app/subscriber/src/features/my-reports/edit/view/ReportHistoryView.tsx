import { Button } from 'components/button';
import { FaX } from 'react-icons/fa6';
import { useReportsStore } from 'store/slices';
import { Col } from 'tno-core';

import * as styled from './styled';

export const ReportHistoryView = () => {
  const [{ reportView }, { storeReportView }] = useReportsStore();

  if (!reportView) return null;

  return (
    <styled.ReportHistoryView className="report-edit-section">
      <div>
        <h1>View Prior Report</h1>
        <Button
          variant="info"
          onClick={() => {
            storeReportView(undefined);
          }}
        >
          <FaX />
        </Button>
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
