import React from 'react';
import { FaPaperPlane } from 'react-icons/fa6';
import { useParams } from 'react-router-dom';
import { useApiHub, useApp, useReportInstances, useReports, useSettings } from 'store/hooks';
import { useUsers } from 'store/hooks/admin';
import {
  AISectionStatusName,
  Button,
  ButtonVariant,
  Col,
  getDistinct,
  type IReportMessageModel,
  type IReportModel,
  type IReportResultModel,
  Loading,
  MessageTargetKey,
  ReportPreviewStatus,
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
  const { editorUrl, subscriberUrl } = useSettings();

  const [isLoading, setIsLoading] = React.useState(true);
  const [view, setView] = React.useState<IReportResultModel | undefined>();
  const [report, setReport] = React.useState<IReportModel>();

  console.error('ReportInstancePreview ');

  const hub = useApiHub();
  const isRequesting = React.useRef(false);

  const handlePreviewReport = React.useCallback(
    async (instanceId: number) => {
      // Only one preview request runs at a time.
      if (isRequesting.current) return;
      try {
        isRequesting.current = true;
        setIsLoading(true);
        const response = await viewReportInstance(instanceId);
        const report = await getReport(response.reportId);
        setView(response);
        setReport(report);
      } catch {
      } finally {
        isRequesting.current = false;
        setIsLoading(false);
      }
    },
    [getReport, viewReportInstance],
  );

  // AI sections are generated in the background; refresh the preview when they are ready.
  hub.useHubEffect(MessageTargetKey.ReportStatus, async (message: IReportMessageModel) => {
    if (message.message === 'ai-sections' && message.id === instanceId) {
      await handlePreviewReport(instanceId);
    }
  });

  // A generator that stops without finishing is caught when its claim lapses.
  const expiresOn = view?.aiSections
    ?.filter((s) => s.status === AISectionStatusName.Generating && s.expiresOn)
    .map((s) => new Date(s.expiresOn!).getTime())
    .sort()[0];
  React.useEffect(() => {
    if (!expiresOn) return;
    const timer = setTimeout(
      () => handlePreviewReport(instanceId),
      Math.max(0, expiresOn - Date.now()) + 5000,
    );
    return () => clearTimeout(timer);
  }, [expiresOn, handlePreviewReport, instanceId]);

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

      // Replace the URL so that it points to the external site.
      let fixed_body = email.body;
      if (editorUrl && subscriberUrl) {
        const urlReplaceRegex = new RegExp(editorUrl, 'gi');
        fixed_body = email.body.replace(urlReplaceRegex, subscriberUrl);
      }

      const htmlBlob = new Blob([fixed_body], { type: 'text/html' });
      const textBlob = new Blob([fixed_body], { type: 'text/plain' });
      const clip = new ClipboardItem({ 'text/html': htmlBlob, 'text/plain': textBlob });
      navigator.clipboard.write([clip]);
      const bcc = subscribers.length > 0 ? `bcc=${emails.join('; ')}` : '';
      window.location.href = `mailto:${to}?${bcc}&subject=${email.subject}&body=Click Paste - Keep Source Formatting`;
    },
    [editorUrl, getDistributionListById, subscriberUrl],
  );

  React.useEffect(() => {
    handlePreviewReport(instanceId);
  }, [handlePreviewReport, instanceId]);

  return (
    <styled.ReportPreview>
      <Show visible={isLoading && !view}>
        <Loading />
      </Show>
      <ReportPreviewStatus
        className="preview-status"
        isRequesting={isLoading}
        sections={view?.aiSections}
      />
      <Show visible={!!view}>
        <Col className="preview-report">
          <div className="preview-subject">
            <div dangerouslySetInnerHTML={{ __html: view?.subject ?? '' }}></div>
            <div>
              <Button
                variant={ButtonVariant.link}
                title="Open Email"
                onClick={async () =>
                  await (userInfo &&
                    report &&
                    view &&
                    prepareEmail(userInfo.preferredEmail ?? userInfo.email, report, view))
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
