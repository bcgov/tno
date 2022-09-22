import { FormPage, IconButton } from 'components/form';
import { ISeriesModel } from 'hooks/api-editor';
import React from 'react';
import { useNavigate } from 'react-router-dom';
import { useSeries } from 'store/hooks/admin';
import { useApp } from 'store/hooks/app/useApp';
import { Col, GridTable, Row } from 'tno-core';

import { columns } from './constants';
import { SeriesListFilter } from './SeriesListFilter';
import * as styled from './styled';

export const SeriesList: React.FC = () => {
  const navigate = useNavigate();
  const [{ requests }] = useApp();
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
            Series provide a way to identify the author, byline or the television series of content.
          </Col>
          <IconButton
            iconType="plus"
            label={`Add new series`}
            onClick={() => navigate(`/admin/series/0`)}
          />
        </Row>
        <GridTable
          columns={columns}
          header={SeriesListFilter}
          manualPageSize
          isLoading={!!requests.length}
          data={items}
          onRowClick={(row) => navigate(`${row.original.id}`)}
        ></GridTable>
      </FormPage>
    </styled.SeriesList>
  );
};
