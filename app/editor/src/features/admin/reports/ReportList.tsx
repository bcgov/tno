import { FormPage } from 'components/formpage';
import React from 'react';
import { useNavigate } from 'react-router-dom';
import { useReports } from 'store/hooks/admin';
import { useAdminStore } from 'store/slices';
import { Col, FlexboxTable, IconButton, IReportModel, Row } from 'tno-core';

import { reportColumns } from './constants';
import { ListFilter } from './ListFilter';
import * as styled from './styled';

export const ReportList: React.FC = () => {
  const navigate = useNavigate();
  const [{ initialized, reports }, api] = useReports();
  const [{ reportFilter }] = useAdminStore();

  const [items, setItems] = React.useState<IReportModel[]>(reports);

  React.useEffect(() => {
    if (!initialized) {
      api.findAllReportsHeadersOnly().catch(() => {});
    }
    // The api will cause a double render because findAllReportsHeadersOnly(...) updates the store.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [initialized]);

  React.useEffect(() => {
    if (reportFilter && reportFilter.length) {
      const value = reportFilter.toLocaleLowerCase();
      setItems(
        reports.filter(
          (i) =>
            i.name.toLocaleLowerCase().includes(value) ||
            i.description.toLocaleLowerCase().includes(value) ||
            i.owner?.username.toLocaleLowerCase().includes(value) ||
            i.owner?.displayName.toLocaleLowerCase().includes(value) ||
            i.owner?.firstName.toLocaleLowerCase().includes(value) ||
            i.owner?.lastName.toLocaleLowerCase().includes(value),
        ),
      );
    } else {
      setItems(reports);
    }
  }, [reportFilter, reports]);

  return (
    <styled.ReportList>
      <FormPage>
        <Row className="add-media" justifyContent="flex-end">
          <Col flex="1 1 0">
            A report generates output based on one ore more filter. A report can be shared by making
            it public, which will allow users to subscribe to it. A template controls how the output
            is presented.
          </Col>
          <IconButton
            iconType="plus"
            label={`Add new report`}
            onClick={() => navigate(`/admin/reports/0`)}
          />
        </Row>
        <ListFilter onFilterChange={(filter) => {}} />
        <FlexboxTable
          rowId="id"
          data={items}
          columns={reportColumns}
          showSort={true}
          onRowClick={(row) => navigate(`${row.original.id}`)}
          pagingEnabled={false}
        />
      </FormPage>
    </styled.ReportList>
  );
};
