import React from 'react';
import { FaPaperPlane } from 'react-icons/fa6';
import { useParams } from 'react-router-dom';
import { useApp, useReportInstances, useReports } from 'store/hooks';
import { useUsers } from 'store/hooks/admin';
import {
  Button,
  ButtonVariant,
  Col,
  getDistinct,
  IReportModel,
  IReportResultModel,
  Loading,
  Show,
  UserAccountTypeName,
} from 'tno-core';

import * as styled from './styled';

const ReportInstancePreview: React.FC = () => {
  const [{ getReport }] = useReports();
  const [{ viewReportInstance }] = useReportInstances();
  const [, { getDistributionListById }] = useUsers();
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
    async (to: string, report: IReportModel, email: IReportResultModel) => {
      const subscribers = report.subscribers
        .filter((s) => s.isSubscribed && s.user?.accountType !== UserAccountTypeName.Distribution)
        .map((s) => s.user);
      const distributions = report.subscribers.filter(
        (s) => s.isSubscribed && s.user?.accountType === UserAccountTypeName.Distribution,
      );

      // Fetch distribution list
      await Promise.all(
        distributions.map(async (distribution) => {
          const users = await getDistributionListById(distribution.userId);
          subscribers.push(...users);
        }),
      );

      const emails = getDistinct(
        subscribers.map((s) => (s?.preferredEmail ? s.preferredEmail : s?.email)),
        (v) => v,
      );

      const htmlBlob = new Blob([email.body], { type: 'text/html' });
      const textBlob = new Blob([email.body], { type: 'text/plain' });
      const clip = new ClipboardItem({ 'text/html': htmlBlob, 'text/plain': textBlob });
      navigator.clipboard.write([clip]);
      const bcc = subscribers.length ? `bcc=${emails.join('; ')}` : '';
      window.location.href = `mailto:${to}?${bcc}&subject=${email.subject}&body=Click Paste - Keep Source Formatting`;
    },
    [getDistributionListById],
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
