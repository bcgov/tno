import React from 'react';
import { useNavigate } from 'react-router-dom';
import { useReports } from 'store/hooks/admin';
import { Col, FlexboxTable, FormPage, IconButton, IReportModel, Row } from 'tno-core';

import { columns } from './constants';
import * as styled from './styled';

export const ReportList: React.FC = () => {
  const navigate = useNavigate();
  const [{ reports }, api] = useReports();

  const [items, setItems] = React.useState<IReportModel[]>([]);

  React.useEffect(() => {
    if (!reports.length) {
      api.findAllReports().then((data) => {
        setItems(data);
      });
    } else {
      setItems(reports);
    }
  }, [api, reports]);

  return (
    <styled.ReportList>
      <FormPage>
        <Row className="add-media" justifyContent="flex-end">
          <Col flex="1 1 0">
            Reports provide a way to generate output (i.e. email) based on a filter or manually
            selecting content to be included.
          </Col>
          <IconButton
            iconType="plus"
            label={`Add new report`}
            onClick={() => navigate(`/admin/reports/0`)}
          />
        </Row>
        <FlexboxTable
          rowId="id"
          data={items}
          columns={columns}
          onRowClick={(row) => navigate(`${row.original.id}`)}
        />
      </FormPage>
    </styled.ReportList>
  );
};
