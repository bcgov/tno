import { Action } from 'components/action';
import { Bar } from 'components/bar';
import { Button } from 'components/button';
import { PageSection } from 'components/section';
import React from 'react';
import { FaArrowsRotate, FaFileCirclePlus, FaPaperPlane, FaPen, FaX } from 'react-icons/fa6';
import { useNavigate } from 'react-router-dom';
import { toast } from 'react-toastify';
import { useReportInstances, useReports } from 'store/hooks';
import { useProfileStore } from 'store/slices';
import { IReportModel, ReportStatusName, Row, Show } from 'tno-core';

import { ReportInstanceView } from './edit/view';

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

  const [isLoading, setIsLoading] = React.useState(false);
  const [isSubmitting, setIsSubmitting] = React.useState(false);

  const instance = report?.instances.length ? report.instances[0] : undefined;

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
    async (report: IReportModel, instanceId: number) => {
      try {
        setIsSubmitting(true);
        const updatedInstance = await publishReportInstance(instanceId);
        onFetch?.({
          ...report,
          instances: report.instances.map((i) =>
            i.id === instanceId ? { ...updatedInstance, content: instance?.content ?? [] } : i,
          ),
        });
        toast.success('Report has been submitted.');
      } catch {
      } finally {
        setIsSubmitting(false);
      }
    },
    [instance?.content, onFetch, publishReportInstance],
  );

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
      </div>
      <Bar>
        <Show
          visible={
            !instance?.sentOn &&
            ![ReportStatusName.Accepted, ReportStatusName.Completed].some(
              (value) => instance?.status === value,
            )
          }
        >
          <Button
            variant="secondary"
            onClick={() => instance?.id && handleRefresh(instance?.id, true).catch(() => {})}
            disabled={isSubmitting}
          >
            Refresh
            <FaArrowsRotate />
          </Button>
          <Button
            onClick={() => navigate(`/reports/${report?.id}/content`)}
            disabled={!report || isSubmitting}
          >
            Edit
            <FaPen />
          </Button>
        </Show>
        <div>
          <Show visible={!!instance}>
            {!instance?.sentOn &&
            ![ReportStatusName.Accepted, ReportStatusName.Completed].some(
              (value) => instance?.status === value,
            ) ? (
              <Button
                onClick={() =>
                  report && instance && handleSend(report, instance.id).catch(() => {})
                }
                disabled={isSubmitting}
              >
                Send
                <FaPaperPlane />
              </Button>
            ) : (
              <Button
                onClick={() => report && handleGenerate(report, true).catch(() => {})}
                disabled={isSubmitting}
                variant="success"
              >
                Start next report
                <FaFileCirclePlus />
              </Button>
            )}
          </Show>
        </div>
      </Bar>
      <ReportInstanceView instanceId={instance?.id ?? 0} />
    </PageSection>
  );
};
