import { Action } from 'components/action';
import { Bar } from 'components/bar';
import { Button } from 'components/button';
import { Modal } from 'components/modal';
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
import { getStatus } from './utils';

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

  const [isLoading, setIsLoading] = React.useState(false);
  const [isSubmitting, setIsSubmitting] = React.useState(false);

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
    if (!!report && !instance && !isLoading) {
      setIsLoading(true);
      fetchReport(report.id)
        .then(async (report) => {
          try {
            if (report && !report?.instances.length) {
              const result = await generateReport(report.id, true);
              onFetch?.(result);
            } else onFetch?.(report);
          } catch (ex) {
            throw ex;
          }
        })
        .finally(() => {
          setIsLoading(false);
        });
    }
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
    async (report: IReportModel, instance: IReportInstanceModel) => {
      try {
        setIsSubmitting(true);
        const updatedInstance = await publishReportInstance(instance.id, !!instance.sentOn);
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
        <Row>Status: {instance ? getStatus(instance.status) : 'Draft'}</Row>
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
            <FaArrowsRotate className="icon-refresh" />
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
        hide={toggleSend}
        type="default"
        confirmText="Yes, Send It"
        onConfirm={() => {
          if (instance) handleSend(report, instance);
          toggleSend();
        }}
      />
    </PageSection>
  );
};
