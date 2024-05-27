import { FormPage } from 'components/formpage';
import React from 'react';
import { useNavigate } from 'react-router-dom';
import { useChartTemplates } from 'store/hooks/admin';
import { Col, FlexboxTable, IChartTemplateModel, IconButton, Row } from 'tno-core';

import { chartTemplateColumns } from './constants';
import { ListFilter } from './ListFilter';
import * as styled from './styled';

export const ChartList: React.FC = () => {
  const navigate = useNavigate();
  const [{ initialized, chartTemplates }, api] = useChartTemplates();

  const [items, setItems] = React.useState<IChartTemplateModel[]>(chartTemplates);

  React.useEffect(() => {
    if (!initialized) {
      api.findAllChartTemplates().then((data) => {
        setItems(data);
      });
    }
    // The api will cause a double render because findAllChartTemplates(...) updates the store.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [initialized]);

  return (
    <styled.ChartList>
      <FormPage>
        <Row className="add-media" justifyContent="flex-end">
          <Col flex="1 1 0">
            <p>
              Templates provide a way to control how charts are generated. Each template provides a
              way to convert data in a report to a JSON object that will be sent to the Charts API.
              The Charts API uses this data to dynamically generate an image. Currently the Charts
              API only supports the{' '}
              <a
                target="_blank"
                href="https://www.chartjs.org/docs/latest/samples/information.html"
                rel="noreferrer"
              >
                Chart.js
              </a>{' '}
              library.
            </p>
          </Col>
          <IconButton
            iconType="plus"
            label={`Add new template`}
            onClick={() => navigate(`/admin/chart/templates/0`)}
          />
        </Row>
        <ListFilter
          onFilterChange={(filter) => {
            if (filter && filter.length) {
              const value = filter.toLocaleLowerCase();
              setItems(
                chartTemplates.filter(
                  (i) =>
                    i.name.toLocaleLowerCase().includes(value) ||
                    i.description.toLocaleLowerCase().includes(value),
                ),
              );
            } else {
              setItems(chartTemplates);
            }
          }}
        />
        <FlexboxTable
          rowId="id"
          data={items}
          columns={chartTemplateColumns}
          showSort={true}
          onRowClick={(row) => navigate(`/admin/chart/templates/${row.original.id}`)}
          pagingEnabled={false}
        />
      </FormPage>
    </styled.ChartList>
  );
};
