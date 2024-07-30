import React from 'react';
import { FaPaperPlane } from 'react-icons/fa6';
import { useParams } from 'react-router-dom';
import { useApp, useReportInstances, useReports } from 'store/hooks';
import {
  Button,
  ButtonVariant,
  Col,
  IReportModel,
  IReportResultModel,
  Loading,
  Show,
} from 'tno-core';

import * as styled from './styled';

const ReportInstancePreview: React.FC = () => {
  const [{ getReport }] = useReports();
  const [{ viewReportInstance }] = useReportInstances();
  const { id } = useParams();
  const instanceId = parseInt(id ?? '');
  const [{ userInfo }] = useApp();

  const [isLoading, setIsLoading] = React.useState(true);
  const [view, setView] = React.useState<IReportResultModel | undefined>();
  const [report, setReport] = React.useState<IReportModel>();

  const handlePreviewReport = React.useCallback(
    async (instanceId: number) => {
      try {
        setIsLoading(true);
        const response = await viewReportInstance(instanceId);
        const report = await getReport(response.reportId);
        setView(response);
        setReport(report);
      } catch {
      } finally {
        setIsLoading(false);
      }
    },
    [getReport, viewReportInstance],
  );

  const prepareEmail = React.useCallback(
    (to: string, report: IReportModel, email: IReportResultModel) => {
      const subscribers = report.subscribers
        .filter((s) => s.isSubscribed)
        .map((s) => (s.user?.preferredEmail ? s.user?.preferredEmail : s.user?.email))
        .filter((s) => s);
      const htmlBlob = new Blob([email.body], { type: 'text/html' });
      const textBlob = new Blob([email.body], { type: 'text/plain' });
      const clip = new ClipboardItem({ 'text/html': htmlBlob, 'text/plain': textBlob });
      navigator.clipboard.write([clip]);
      window.location.href = `mailto:${to}?bcc=${subscribers.join('; ')}&subject=${
        email.subject
      }&body=Click Paste - Keep Source Formatting`;
    },
    [],
  );

  React.useEffect(() => {
    handlePreviewReport(instanceId);
  }, [handlePreviewReport, instanceId]);

  return (
    <styled.ReportPreview>
      <Show visible={isLoading}>
        <Loading />
      </Show>
      <Show visible={!isLoading}>
        <Col className="preview-report">
          <div className="preview-subject">
            <div dangerouslySetInnerHTML={{ __html: view?.subject ?? '' }}></div>
            <div>
              <Button
                variant={ButtonVariant.link}
                title="Open Email"
                onClick={() =>
                  userInfo &&
                  report &&
                  view &&
                  prepareEmail(userInfo.preferredEmail ?? userInfo.email, report, view)
                }
              >
                <FaPaperPlane />
              </Button>
            </div>
          </div>
          <div
            className="preview-body"
            dangerouslySetInnerHTML={{ __html: view?.body ?? '' }}
          ></div>
        </Col>
      </Show>
    </styled.ReportPreview>
  );
};

export default ReportInstancePreview;
