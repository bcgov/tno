import { FormPage } from 'components/formpage';
import React from 'react';
import { useNavigate } from 'react-router-dom';
import { useReportTemplates } from 'store/hooks/admin';
import { Col, FlexboxTable, IconButton, IReportTemplateModel, Row } from 'tno-core';

import { reportTemplateColumns } from './constants';
import { ListFilter } from './ListFilter';
import * as styled from './styled';

export const ReportTemplateList: React.FC = () => {
  const navigate = useNavigate();
  const [{ initialized, reportTemplates }, api] = useReportTemplates();

  const [items, setItems] = React.useState<IReportTemplateModel[]>(reportTemplates);

  React.useEffect(() => {
    if (!initialized) {
      api.findAllReportTemplates().then((data) => {
        setItems(data);
      });
    }
    // The api will cause a double render because findAllReportTemplates(...) updates the store.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [initialized]);

  return (
    <styled.ReportList>
      <FormPage>
        <Row justifyContent="flex-end">
          <Col flex="1 1 0">
            Templates use the Razor syntax to generate the report. Each template controls what
            configuration options are available.
          </Col>
          <IconButton
            iconType="plus"
            label={`Add new template`}
            onClick={() => navigate(`/admin/report/templates/0`)}
          />
        </Row>
        <ListFilter
          onFilterChange={(filter) => {
            if (filter && filter.length) {
              const value = filter.toLocaleLowerCase();
              setItems(
                reportTemplates.filter(
                  (i) =>
                    i.name.toLocaleLowerCase().includes(value) ||
                    i.description.toLocaleLowerCase().includes(value),
                ),
              );
            } else {
              setItems(reportTemplates);
            }
          }}
        />
        <FlexboxTable
          rowId="id"
          data={items}
          columns={reportTemplateColumns}
          showSort={true}
          onRowClick={(row) => navigate(`/admin/report/templates/${row.original.id}`)}
          pagingEnabled={false}
        />
      </FormPage>
    </styled.ReportList>
  );
};
