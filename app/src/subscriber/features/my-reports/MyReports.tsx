import { Action } from '@/subscriber/components/action';
import { Bar } from '@/subscriber/components/bar';
import { Modal } from '@/subscriber/components/modal';
import { PageSection } from '@/subscriber/components/section';
import React from 'react';
import { useNavigate } from 'react-router-dom';
import { toast } from 'react-toastify';
import { useApiHub, useApp, useReportInstances, useReports } from '@/subscriber/store/hooks';
import {
  Button,
  IReportMessageModel,
  IReportModel,
  Loader,
  MessageTargetKey,
  Row,
  useModal,
} from 'tno-core';

import { ReportCard } from './ReportCard';
import { ReportFilter } from './ReportFilter';
import { ReportPreview } from './ReportPreview';
import * as styled from './styled';
import { isAISectionTestMessage } from './utils';

/** How long to wait before asking again whether the list of reports is stale, in milliseconds. */
const staleCheckInterval = 30000;

export const MyReports: React.FC = () => {
  const [{ myReports, reportsFilter }, { findMyReports, deleteReport, getReport }] = useReports();
  const [{ getReportInstance }] = useReportInstances();
  const { toggle, isShowing } = useModal();
  const navigate = useNavigate();
  const [{ requests, userInfo }] = useApp();
  const hub = useApiHub();

  const [report, setReport] = React.useState<IReportModel>();

  const applyFilter = React.useCallback((reports: IReportModel[], filter: string) => {
    return reports.filter((r) => r.name.toLowerCase().includes(filter.toLowerCase()));
  }, []);

  // The list is kept while the app stays open, so it is checked for changes every time this page
  // is opened. Only a list that has changed is fetched again.
  const refresh = React.useRef(findMyReports);
  refresh.current = findMyReports;

  React.useEffect(() => {
    if (userInfo) refresh.current().catch(() => {});
  }, [userInfo]);

  React.useEffect(() => {
    // A tab left open is checked again when the user comes back to it.
    let checkedOn = Date.now();
    const check = () => {
      if (document.visibilityState !== 'visible' || Date.now() - checkedOn < staleCheckInterval)
        return;
      checkedOn = Date.now();
      refresh.current().catch(() => {});
    };
    document.addEventListener('visibilitychange', check);
    window.addEventListener('focus', check);
    return () => {
      document.removeEventListener('visibilitychange', check);
      window.removeEventListener('focus', check);
    };
  }, []);

  hub.useHubEffect(MessageTargetKey.ReportStatus, async (message: IReportMessageModel) => {
    // A finished AI section test run is for the prompt workbench; nothing about the report changed.
    if (isAISectionTestMessage(message)) return;
    if (report) {
      try {
        if (message.message === 'status') {
          setReport({
            ...report,
            instances: report.instances.map((i) =>
              i.id === message.id ? { ...i, status: message.status, version: message.version } : i,
            ),
          });
        } else if (message.message === 'event') {
          const updateReport = await getReport(report.id, false);
          if (updateReport) {
            setReport({
              ...report,
              events: updateReport.events,
              version: updateReport.version,
            });
          }
        } else {
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

  // Checking a list that is already on the page happens behind it.
  const isLoading = !myReports.length && requests.some((r) => r.url === 'find-my-reports');

  return (
    <styled.MyReports>
      <PageSection header="My Reports" includeHeaderIcon className="my-reports">
        <Bar>
          <Row flex="1" justifyContent="flex-end">
            <Button className="create-new-button">
              <Action
                label="New Report"
                variant="create-new"
                onClick={() => navigate('/reports/0')}
              />
            </Button>
            <ReportFilter />
          </Row>
        </Bar>
        <div className="my-reports-content">
          <Loader visible={isLoading} />
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
        onClose={toggle}
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
