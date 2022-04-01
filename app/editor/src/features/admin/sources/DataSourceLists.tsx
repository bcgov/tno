import { IDataSourceModel } from 'hooks/api-editor';
import { toPage } from 'hooks/api-editor/utils';
import React from 'react';
import { useNavigate } from 'react-router-dom';
import { useDataSources } from 'store/hooks/admin/sources';
import { defaultPage, GridTable, IPage } from 'tno-core/dist/components/grid-table';

import { DataSourceFilter } from '.';
import { columns } from './constants';
import * as styled from './styled';

interface IDataSourceListProps {}

export const DataSourceList: React.FC<IDataSourceListProps> = (props) => {
  const navigate = useNavigate();
  const [{ dataSources }, api] = useDataSources();

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
        data={page.items}
        header={DataSourceFilter}
        onRowClick={(row) => navigate(`${row.original.id}`)}
      ></GridTable>
    </styled.DataSourceList>
  );
};
