import { FormPage } from 'components/formpage';
import React from 'react';
import { useNavigate } from 'react-router-dom';
import { useSeries } from 'store/hooks/admin';
import { Col, FlexboxTable, IconButton, ISeriesModel, Row } from 'tno-core';

import { columns } from './constants';
import { SeriesFilter } from './SeriesFilter';
import * as styled from './styled';

const SeriesList: React.FC = () => {
  const navigate = useNavigate();
  const [{ series }, api] = useSeries();

  const [items, setItems] = React.useState<ISeriesModel[]>([]);

  React.useEffect(() => {
    if (!series.length) {
      api.findAllSeries().then((data) => {
        setItems(data);
      });
    } else {
      setItems(series);
    }
  }, [api, series]);

  return (
    <styled.SeriesList>
      <FormPage>
        <Row className="add-media" justifyContent="flex-end">
          <Col flex="1 1 0">
            Show/Program provides a way to identify the author, byline or the television series of
            content.
          </Col>
          <IconButton
            iconType="plus"
            label={`Add new show/program`}
            onClick={() => navigate(`/admin/programs/0`)}
          />
        </Row>
        <SeriesFilter
          onFilterChange={(filter) => {
            if (filter && filter.length) {
              const value = filter.toLocaleLowerCase();
              setItems(
                series.filter(
                  (i) =>
                    i.name.toLocaleLowerCase().includes(value) ||
                    i.description.toLocaleLowerCase().includes(value) ||
                    i.source?.name.toLocaleLowerCase().includes(value) ||
                    i.source?.code.toLocaleLowerCase().includes(value),
                ),
              );
            } else {
              setItems(series);
            }
          }}
        />
        <FlexboxTable
          rowId="id"
          data={items}
          columns={columns}
          showSort={true}
          onRowClick={(row) => navigate(`${row.original.id}`)}
          pagingEnabled={false}
        />
      </FormPage>
    </styled.SeriesList>
  );
};

export default SeriesList;
