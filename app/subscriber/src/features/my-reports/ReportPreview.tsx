import { Action } from 'components/action';
import { Bar } from 'components/bar';
import { Button } from 'components/button';
import { Modal } from 'components/modal';
import { RefreshButton } from 'components/refresh-button/styled';
import { PageSection } from 'components/section';
import React from 'react';
import { FaArrowsRotate, FaFileCirclePlus, FaPaperPlane, FaPen, FaX } from 'react-icons/fa6';
import { useNavigate } from 'react-router-dom';
import { toast } from 'react-toastify';
import { useReportInstances, useReports } from 'store/hooks';
import { useProfileStore } from 'store/slices';
import {
  Col,
  getReportKind,
  IReportInstanceModel,
  IReportModel,
  ReportKindName,
  ReportStatusName,
  Row,
  Show,
  Spinner,
  useModal,
} from 'tno-core';

import { ReportKindIcon } from './components';
import { ReportInstanceView } from './ReportInstanceView';
import { ReportStatus } from './ReportStatus';

export interface IReportPreviewProps {
  report?: IReportModel;
  onFetch?: (report?: IReportModel) => void;
  onClose?: () => void;
}

export const ReportPreview = ({ report, onFetch, onClose }: IReportPreviewProps) => {
  const navigate = useNavigate();
  const [{ viewReportInstance, publishReportInstance }] = useReportInstances();
  const [, { storeReportOutput }] = useProfileStore();
  const [, { getReport, generateReport }] = useReports();
  const { isShowing: showSend, toggle: toggleSend } = useModal();
  const { isShowing: showResend, toggle: toggleResend } = useModal();

  const [isLoading, setIsLoading] = React.useState(false);
  const [isSubmitting, setIsSubmitting] = React.useState(false);
  const autoGenerateRef = React.useRef<number | null>(null);

  const instance = report?.instances.length ? report.instances[0] : undefined;
  const hasSubscribers = report?.subscribers?.some((s) => s.isSubscribed === true) ?? false;

  const fetchReport = React.useCallback(
    async (id: number) => {
      try {
        const report = await getReport(id);
        onFetch?.(report);
        return report;
      } catch {}
    },
    [getReport, onFetch],
  );

  // If there is no instance first fetch the report information to see if there should be one.
  // If there still is no instance, then it must be initialized.
  React.useEffect(() => {
    if (!report) {
      autoGenerateRef.current = null;
      return;
    }

    const reportId = report.id;

    if (instance) {
      if (autoGenerateRef.current === reportId) autoGenerateRef.current = null;
      return;
    }

    if (isLoading || autoGenerateRef.current === reportId) return;

    autoGenerateRef.current = reportId;
    setIsLoading(true);
    fetchReport(report.id)
      .then(async (report) => {
        try {
          if (report && !report?.instances.length) {
            const result = await generateReport(report.id, true);
            onFetch?.(result);
          } else onFetch?.(report);
        } catch (ex) {
          autoGenerateRef.current = null;
          throw ex;
        }
      })
      .catch(() => {
        autoGenerateRef.current = null;
      })
      .finally(() => {
        setIsLoading(false);
      });
  }, [fetchReport, generateReport, instance, isLoading, onFetch, report]);
  const handleRefresh = React.useCallback(
    async (instanceId: number, regenerate?: boolean) => {
      try {
        setIsSubmitting(true);
        const response = await viewReportInstance(instanceId, regenerate);
        storeReportOutput({ ...response, instanceId });
      } catch {
      } finally {
        setIsSubmitting(false);
      }
    },
    [viewReportInstance, storeReportOutput],
  );

  const handleGenerate = React.useCallback(
    async (report: IReportModel, generate: boolean) => {
      try {
        setIsSubmitting(true);
        await generateReport(report.id, generate);
        navigate(`/reports/${report.id}/content`);
      } catch {
      } finally {
        setIsSubmitting(false);
      }
    },
    [generateReport, navigate],
  );

  const handleSend = React.useCallback(
    async (report: IReportModel, instance: IReportInstanceModel, resend: boolean) => {
      try {
        setIsSubmitting(true);
        const updatedInstance = await publishReportInstance(instance.id, resend);
        onFetch?.({
          ...report,
          instances: report.instances.map((i) =>
            i.id === instance.id ? { ...updatedInstance, content: instance?.content ?? [] } : i,
          ),
        });
        toast.success('Report has been submitted.');
      } catch {
      } finally {
        setIsSubmitting(false);
      }
    },
    [onFetch, publishReportInstance],
  );

  if (!report) return null;

  return (
    <PageSection
      className="report-preview"
      header={
        <Row flex="1" justifyContent="space-between">
          <div>Report Preview</div>
          <Action icon={<FaX className="icon-close" />} onClick={() => onClose?.()} />
        </Row>
      }
    >
      <div className="report-title">
        <h2>{report?.name}</h2>
        <ReportKindIcon report={report} />
      </div>
      <Bar>
        <Row>
          Status: <ReportStatus status={instance?.status} />
        </Row>
        <Show visible={instance?.status === ReportStatusName.Submitted}>
          <Col>
            <Spinner />
          </Col>
        </Show>
        <Show
          visible={
            instance &&
            !instance.sentOn &&
            ![
              ReportStatusName.Submitted,
              ReportStatusName.Accepted,
              ReportStatusName.Completed,
            ].includes(instance.status)
          }
        >
          <Button
            variant="secondary"
            onClick={() => instance?.id && handleRefresh(instance?.id, true).catch(() => {})}
            disabled={isSubmitting}
          >
            Refresh
            <RefreshButton icon={<FaArrowsRotate />} />
          </Button>
        </Show>
        <Show visible={!!instance?.sentOn}>
          <Button
            onClick={() => navigate(`/reports/${report?.id}`)}
            disabled={!report || isSubmitting}
          >
            Edit Settings
            <FaPen />
          </Button>
        </Show>
        <Show visible={!instance?.sentOn}>
          <Button
            onClick={() => navigate(`/reports/${report?.id}/content`)}
            disabled={!report || isSubmitting}
          >
            Edit
            <FaPen />
          </Button>
        </Show>
        <Show visible={!!instance}>
          <Show
            visible={
              instance &&
              !instance.sentOn &&
              ![ReportStatusName.Submitted].includes(instance.status)
            }
          >
            <Button
              onClick={() => report && instance && toggleSend()}
              disabled={isSubmitting || !hasSubscribers}
              title={
                !hasSubscribers
                  ? 'There are no subscribers for this report. Add subscribers to enable sending.'
                  : ''
              }
            >
              Send
              <FaPaperPlane />
            </Button>
          </Show>
          <Show visible={instance && [ReportStatusName.Failed].includes(instance.status)}>
            <Button
              onClick={() => report && instance && toggleResend()}
              disabled={isSubmitting || !hasSubscribers}
              variant="warn"
              title={
                !hasSubscribers
                  ? 'There are no subscribers for this report. Add subscribers to enable sending.'
                  : ''
              }
            >
              Retry
              <FaPaperPlane />
            </Button>
          </Show>
          <Show visible={!!instance?.sentOn}>
            <Show visible={getReportKind(report) === ReportKindName.Manual}>
              <Button
                onClick={() => report && handleGenerate(report, true).catch(() => {})}
                disabled={isSubmitting}
                variant="success"
              >
                Start next report
                <FaFileCirclePlus />
              </Button>
            </Show>
          </Show>
        </Show>
      </Bar>
      <ReportInstanceView instanceId={instance?.id ?? 0} />
      <Modal
        headerText="Confirm Send"
        body={`Do you want to send this report to all subscribers?`}
        isShowing={showSend}
        onClose={toggleSend}
        type="default"
        confirmText="Yes, Send It"
        onConfirm={() => {
          if (instance) handleSend(report, instance, !!instance.sentOn);
          toggleSend();
        }}
      />
      <Modal
        headerText="Confirm Retry"
        body={`Do you want to retry sending this report to subscribers who have not received the report?`}
        isShowing={showResend}
        onClose={toggleResend}
        type="default"
        confirmText="Yes, Send It"
        onConfirm={() => {
          if (instance) handleSend(report, instance, false);
          toggleResend();
        }}
      />
    </PageSection>
  );
};
