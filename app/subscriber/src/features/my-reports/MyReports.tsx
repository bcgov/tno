import { Action } from 'components/action';
import { Bar } from 'components/bar';
import { Header } from 'components/header';
import { PageSection } from 'components/section';
import React from 'react';
import { FaClipboard } from 'react-icons/fa6';
import { useNavigate } from 'react-router-dom';
import { toast } from 'react-toastify';
import { useApp, useReports } from 'store/hooks';
import { useProfileStore } from 'store/slices';
import { Col, IReportModel, Loading, Modal, Row, useModal } from 'tno-core';

import { ReportCard } from './ReportCard';
import { ReportFilter } from './ReportFilter';
import * as styled from './styled';

export const MyReports: React.FC = () => {
  const [{ myReports, reportsFilter }] = useProfileStore();
  const [{ findMyReports, deleteReport }] = useReports();
  const { toggle, isShowing } = useModal();
  const navigate = useNavigate();
  const [{ requests }] = useApp();

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
      <Header />
      <PageSection header="My Reports">
        <Bar>
          <ReportFilter />
          <Row flex="1" justifyContent="flex-end">
            <Action
              label="Create New"
              icon={<FaClipboard />}
              onClick={() => navigate('/reports/0/settings')}
            />
          </Row>
        </Bar>
        <div>
          {isLoading && (
            <Col className="loading">
              <Loading />
            </Col>
          )}
          {applyFilter(myReports, reportsFilter).map((report) => {
            return (
              <ReportCard
                key={report.id}
                report={report}
                onDelete={(report) => {
                  setReport(report);
                  toggle();
                }}
              />
            );
          })}
        </div>
      </PageSection>
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
