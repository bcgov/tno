import React from 'react';
import { useParams } from 'react-router-dom';
import { useReports } from 'store/hooks';
import { Col, IReportResultModel, Loading, Show } from 'tno-core';

import * as styled from './styled';

const ReportPreview: React.FC = () => {
  const [{ previewReport }] = useReports();
  const { id } = useParams();
  const reportId = parseInt(id ?? '');

  const [isLoading, setIsLoading] = React.useState(true);
  const [preview, setPreview] = React.useState<IReportResultModel | undefined>();

  const handlePreviewReport = React.useCallback(
    async (reportId: number) => {
      try {
        setIsLoading(true);
        const response = await previewReport(reportId);
        setPreview(response);
      } catch {
      } finally {
        setIsLoading(false);
      }
    },
    [previewReport, setPreview],
  );

  React.useEffect(() => {
    handlePreviewReport(reportId);
  }, [handlePreviewReport, reportId]);

  return (
    <styled.ReportPreview>
      <Show visible={isLoading}>
        <Loading />
      </Show>
      <Show visible={!isLoading}>
        <Col className="preview-report">
          <div
            className="preview-subject"
            dangerouslySetInnerHTML={{ __html: preview?.subject ?? '' }}
          ></div>
          <div
            className="preview-body"
            dangerouslySetInnerHTML={{ __html: preview?.body ?? '' }}
          ></div>
        </Col>
      </Show>
    </styled.ReportPreview>
  );
};

export default ReportPreview;
