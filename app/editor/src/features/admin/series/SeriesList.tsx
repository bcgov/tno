import { FormPage } from 'components/form/formpage/styled';
import { ISeriesModel } from 'hooks/api-editor';
import React from 'react';
import { useNavigate } from 'react-router-dom';
import { useSeries } from 'store/hooks/admin';
import { useApp } from 'store/hooks/app/useApp';
import { GridTable } from 'tno-core';

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
      api.findAllSeriess().then((data) => {
        setItems(data);
      });
    } else {
      setItems(series);
    }
  }, [api, series]);

  return (
    <styled.SeriesList>
      <FormPage>
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
