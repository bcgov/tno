import { defaultPage, GridTable, IPage, toPage } from 'components/grid-table';
import { IDataSourceModel } from 'hooks/api-editor';
import React from 'react';
import { useNavigate } from 'react-router-dom';
import { useDataSources } from 'store/hooks/admin/sources';
import { useApp } from 'store/hooks/app/useApp';

import { DataSourceFilter } from '.';
import { columns } from './constants';
import * as styled from './styled';

interface IDataSourceListProps {}

export const DataSourceList: React.FC<IDataSourceListProps> = (props) => {
  const navigate = useNavigate();
  const [{ dataSources }, api] = useDataSources();
  const [{ requests }] = useApp();

  const [page, setPage] = React.useState<IPage<IDataSourceModel>>(defaultPage<IDataSourceModel>());

  React.useEffect(() => {
    if (dataSources.length) {
      setPage(
        toPage({
          page: 1,
          items: dataSources,
          quantity: 10,
          total: dataSources.length,
        }),
      );
    } else {
      api.findDataSources().then((data) => {
        setPage(toPage(data));
      });
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  return (
    <styled.DataSourceList>
      <GridTable
        columns={columns}
        isLoading={!!requests.length}
        data={page.items}
        header={DataSourceFilter}
        onRowClick={(row) => navigate(`${row.original.id}`)}
      ></GridTable>
    </styled.DataSourceList>
  );
};
