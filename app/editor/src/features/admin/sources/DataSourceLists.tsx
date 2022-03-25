import { defaultPage, GridTable, IPage, toPage } from 'components/grid-table';
import { IDataSourceModel } from 'hooks/api-editor';
import React from 'react';
import { useNavigate } from 'react-router-dom';
import { useDataSources } from 'store/hooks/admin/sources';

import { DataSourceFilter } from '.';
import { columns } from './constants';
import * as styled from './styled';

interface IDataSourceListProps {}

export const DataSourceList: React.FC<IDataSourceListProps> = (props) => {
  const navigate = useNavigate();
  const [api] = useDataSources();

  const [page, setPage] = React.useState<IPage<IDataSourceModel>>(defaultPage<IDataSourceModel>());

  React.useEffect(() => {
    api.findDataSources().then((data) => {
      setPage(toPage(data));
    });
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
