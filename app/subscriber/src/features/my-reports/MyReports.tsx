import React from 'react';
import { Link } from 'react-router-dom';
import { toast } from 'react-toastify';
import { useReports } from 'store/hooks';
import { Col, FlexboxTable, IReportModel, Modal, Row, useModal } from 'tno-core';

import { reportProductColumns } from './constants';
import { useColumns } from './hooks';
import * as styled from './styled';
import { isAutoSend } from './utils';

export const MyReport: React.FC = () => {
  const [{ getPublicReports, findMyReports, deleteReport, updateReport }] = useReports();
  const { toggle, isShowing } = useModal();

  const [myReports, setMyReports] = React.useState<IReportModel[]>([]);
  const [allReports, setAllReports] = React.useState<IReportModel[]>([]);
  const [report, setReport] = React.useState<IReportModel>();

  React.useEffect(() => {
    findMyReports().then((data) => {
      setMyReports(data);
    });
    getPublicReports().then((data) => {
      setAllReports(data);
    });
    // Only do this on init.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const handleDelete = React.useCallback(
    (report: IReportModel) => {
      if (!!report) {
        deleteReport(report)
          .then((data) => {
            toast.success(`Successfully deleted '${data.name}' report.`);
            setMyReports(myReports.filter((r) => r.id !== report.id));
          })
          .catch();
      }
    },
    [deleteReport, myReports],
  );

  const columns = useColumns(
    async (report) => {
      try {
        const autoSend = isAutoSend(report);
        const result = await updateReport(report);
        setMyReports(myReports.map((r) => (r.id !== report.id ? r : result)));
        toast.success(`Auto send has been turned ${autoSend ? 'on' : 'off'}.`);
      } catch {}
    },
    (report) => {
      setReport(report);
      toggle();
    },
  );

  return (
    <styled.MyReports>
      <Col className="my-reports">
        <Row className="header">
          <div>Reports I can edit: </div>
          <Link className="create-new" to={'/reports/0'}>
            + Create new
          </Link>
        </Row>
        <FlexboxTable
          pagingEnabled={false}
          columns={columns}
          rowId={'id'}
          data={myReports}
          showActive={false}
        />
        <Col className="info">
          <Row>Media Monitoring products</Row>
          <Row>
            The following automated reports are available for subscription. Subscribed reports
            delivered by email.
          </Row>
        </Col>
        <FlexboxTable
          pagingEnabled={false}
          columns={reportProductColumns}
          rowId={'id'}
          data={allReports}
          showActive={false}
        />
      </Col>
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
