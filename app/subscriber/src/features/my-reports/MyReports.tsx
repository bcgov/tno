import { Action } from 'components/action';
import { Bar } from 'components/bar';
import { PageSection } from 'components/section';
import React from 'react';
import { FaClipboard } from 'react-icons/fa6';
import { useNavigate } from 'react-router-dom';
import { toast } from 'react-toastify';
import { useApiHub, useApp, useReportInstances, useReports } from 'store/hooks';
import {
  IReportMessageModel,
  IReportModel,
  Loading,
  MessageTargetName,
  Modal,
  ReportStatusName,
  Row,
  Show,
  useModal,
} from 'tno-core';

import { ReportCard } from './ReportCard';
import { ReportFilter } from './ReportFilter';
import { ReportPreview } from './ReportPreview';
import * as styled from './styled';

export const MyReports: React.FC = () => {
  const [{ myReports, reportsFilter }, { findMyReports, deleteReport }] = useReports();
  const [{ getReportInstance }] = useReportInstances();
  const { toggle, isShowing } = useModal();
  const navigate = useNavigate();
  const [{ requests }] = useApp();
  const hub = useApiHub();

  const [report, setReport] = React.useState<IReportModel>();

  const applyFilter = React.useCallback((reports: IReportModel[], filter: string) => {
    return reports.filter((r) => r.name.toLowerCase().includes(filter.toLowerCase()));
  }, []);

  React.useEffect(() => {
    if (!myReports.length) {
      findMyReports().catch(() => {});
    }
    // Only do this on init.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  hub.useHubEffect(MessageTargetName.ReportStatus, async (message: IReportMessageModel) => {
    if (report) {
      try {
        if (message.status === ReportStatusName.Accepted) {
          const instance = await getReportInstance(message.id, false);
          if (instance) {
            setReport({
              ...report,
              instances: report.instances.map((i) => (i.id === message.id ? instance : i)),
            });
          }
        }
      } catch {}
    }
  });

  const handleDelete = React.useCallback(
    (report: IReportModel) => {
      if (!!report) {
        deleteReport(report)
          .then((data) => {
            toast.success(`Successfully deleted '${data.name}' report.`);
          })
          .catch(() => {});
      }
    },
    [deleteReport],
  );

  const isLoading = requests.some((r) => r.url === 'find-my-reports');

  return (
    <styled.MyReports>
      <PageSection header="My Reports" includeHeaderIcon className="my-reports">
        <Bar>
          <ReportFilter />
          <Row flex="1" justifyContent="flex-end">
            <Action
              label="Create New"
              icon={<FaClipboard />}
              onClick={() => navigate('/reports/0')}
            />
          </Row>
        </Bar>
        <div className="my-reports-content">
          <Show visible={isLoading}>
            <Loading />
          </Show>
          {applyFilter(myReports, reportsFilter).map((item) => {
            return (
              <ReportCard
                className={report?.id === item.id ? 'active' : ''}
                key={item.id}
                report={item}
                onClick={(report) => setReport(report)}
                onDelete={(report) => {
                  setReport(report);
                  toggle();
                }}
              />
            );
          })}
        </div>
      </PageSection>
      {report && (
        <ReportPreview
          report={report}
          onFetch={(report) => setReport(report)}
          onClose={() => setReport(undefined)}
        />
      )}
      <Modal
        headerText="Confirm Delete"
        body={`Are you sure you wish to delete the '${report?.name}' report?`}
        isShowing={isShowing}
        hide={toggle}
        type="delete"
        confirmText="Yes, Remove It"
        onConfirm={() => {
          if (report) handleDelete(report);
          toggle();
        }}
      />
    </styled.MyReports>
  );
};
