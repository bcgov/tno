import React from 'react';
import { toast } from 'react-toastify';
import { Tooltip } from 'react-tooltip';
import { useReports } from 'store/hooks/subscriber/useReports';
import { Col, FlexboxTable, IReportModel, Row } from 'tno-core';

import { availableReportColumns } from './constants/availableReportColumns';
import { columns } from './constants/columns';
import * as styled from './styled';

export const MyReport: React.FC = () => {
  const [{ findAllReports, findMyReports, addReport, updateReport }] = useReports();
  const [myReports, setMyReports] = React.useState<IReportModel[]>([]);
  const [allReports, setAllReports] = React.useState<IReportModel[]>([]);

  const [active, setActive] = React.useState<IReportModel>();
  const [editable, setEditable] = React.useState<string>('');
  React.useEffect(() => {
    findMyReports().then((data) => {
      setMyReports(data);
    });
    findAllReports().then((data) => {
      setAllReports(data);
    });
    // Only do this on init.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const handleSave = () => {
    if (!!active) {
      updateReport(active).then((data) => {
        toast.success(`${data.name} updated successfully`);
        setMyReports([...myReports.filter((report) => report.id !== data.id), data]);
        setEditable('');
      });
    }
  };

  return (
    <styled.MyReports>
      <Col className="my-reports">
        <Row className="header">
          <div>Reports I can edit: </div>
          <div className="create-new">+ Create new</div>
        </Row>
        <FlexboxTable
          pagingEnabled={false}
          columns={columns(setActive, editable, handleSave, active)}
          rowId={'id'}
          data={myReports}
          showActive={false}
        />
        <Tooltip
          clickable
          openOnClick
          place="right"
          id="options"
          variant="light"
          className="options"
        >
          <Col className="folder-container">
            <div className="option" onClick={() => setEditable(active?.name ?? '')}>
              Edit report
            </div>
            <div
              className="option"
              onClick={() => {
                if (!!active) {
                  updateReport({ ...active }).then(() => {
                    toast.success(`${active.name} updated successfully`);
                  });
                }
              }}
            >
              Share report
            </div>
          </Col>
        </Tooltip>
        <Col className="info">
          <Row>Media Monitoring products</Row>
          <Row>
            The following automated reports are available for subscription. Subscribed reports
            delivered by email.
          </Row>
        </Col>
        <FlexboxTable
          pagingEnabled={false}
          columns={availableReportColumns(setActive, editable, handleSave, active)}
          rowId={'id'}
          data={allReports}
          showActive={false}
        />
      </Col>
    </styled.MyReports>
  );
};
