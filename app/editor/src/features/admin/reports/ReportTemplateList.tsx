import React from 'react';
import { useNavigate } from 'react-router-dom';
import { useReportTemplates } from 'store/hooks/admin';
import { Col, FlexboxTable, FormPage, IconButton, IReportTemplateModel, Row } from 'tno-core';

import { templateColumns } from './constants';
import { ReportFilter } from './ReportFilter';
import * as styled from './styled';

export const ReportTemplateList: React.FC = () => {
  const navigate = useNavigate();
  const [{ reportTemplates }, api] = useReportTemplates();

  const [items, setItems] = React.useState<IReportTemplateModel[]>([]);

  React.useEffect(() => {
    if (!reportTemplates.length) {
      api.findAllReportTemplates().then((data) => {
        setItems(data);
      });
    } else {
      setItems(reportTemplates);
    }
  }, [api, reportTemplates]);

  return (
    <styled.ReportList>
      <FormPage>
        <Row className="add-media" justifyContent="flex-end">
          <Col flex="1 1 0">
            Report Templates provide a way to generate output (i.e. email) through a Razor syntax.
          </Col>
          <IconButton
            iconType="plus"
            label={`Add new template`}
            onClick={() => navigate(`/admin/report/templates/0`)}
          />
        </Row>
        <ReportFilter
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
          columns={templateColumns}
          showSort={true}
          onRowClick={(row) => navigate(`/admin/report/templates/${row.original.id}`)}
          pagingEnabled={false}
        />
      </FormPage>
    </styled.ReportList>
  );
};
