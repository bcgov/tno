import { Action } from 'components/action';
import { Bar } from 'components/bar';
import { Header } from 'components/header';
import { PageSection } from 'components/section';
import React from 'react';
import { FaClipboard } from 'react-icons/fa6';
import { useNavigate } from 'react-router-dom';
import { toast } from 'react-toastify';
import { useReports } from 'store/hooks';
import { useProfileStore } from 'store/slices';
import { IReportModel, Modal, Row, useModal } from 'tno-core';

import * as styled from './styled/MyColleagues';

export const MyColleagues: React.FC = () => {
  const [{ myReports, reportsFilter }] = useProfileStore();
  const [{ findMyReports, deleteReport }] = useReports();
  const { toggle, isShowing } = useModal();
  const navigate = useNavigate();

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

  return (
    <styled.MyColleagues>
      <Header />
      <PageSection header="My Colleagues">
        <Bar>
          <Row flex="1" justifyContent="flex-end">
            <Action
              label="Add Colleague"
              icon={<FaClipboard />}
              onClick={() => navigate('/colleague/0/settings')}
            />
          </Row>
        </Bar>
        <div>
          {applyFilter(myReports, reportsFilter).map((report) => {
            return <p>Colleague</p>;
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
    </styled.MyColleagues>
  );
};
