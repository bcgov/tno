import React from 'react';
import { useNavigate } from 'react-router-dom';
import { useReports } from 'store/hooks/admin';
import { useApp } from 'store/hooks/app/useApp';
import { Col, FormPage, GridTable, IconButton, IReportModel, Row } from 'tno-core';

import { columns } from './constants';
import { ReportListFilter } from './ReportListFilter';
import * as styled from './styled';

export const ReportList: React.FC = () => {
  const navigate = useNavigate();
  const [{ requests }] = useApp();
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
        <GridTable
          columns={columns}
          header={ReportListFilter}
          paging={{ pageSizeOptions: { fromLocalStorage: true } }}
          isLoading={!!requests.length}
          data={items}
          onRowClick={(row) => navigate(`${row.original.id}`)}
        ></GridTable>
      </FormPage>
    </styled.ReportList>
  );
};
