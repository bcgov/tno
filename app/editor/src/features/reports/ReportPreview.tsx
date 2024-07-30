import React from 'react';
import { FaCopy } from 'react-icons/fa6';
import { useParams } from 'react-router-dom';
import { useReports } from 'store/hooks';
import { Button, ButtonVariant, Col, IReportResultModel, Loading, Show } from 'tno-core';

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

  const copy = React.useCallback((report: IReportResultModel) => {
    const htmlBlob = new Blob([report.body], { type: 'text/html' });
    const textBlob = new Blob([report.body], { type: 'text/plain' });
    const clip = new ClipboardItem({ 'text/html': htmlBlob, 'text/plain': textBlob });
    navigator.clipboard.write([clip]);
  }, []);

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
          <div className="preview-subject">
            <div dangerouslySetInnerHTML={{ __html: preview?.subject ?? '' }}></div>
            <div>
              <Button
                variant={ButtonVariant.link}
                title="Copy"
                onClick={() => preview && copy(preview)}
              >
                <FaCopy />
              </Button>
            </div>
          </div>
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
