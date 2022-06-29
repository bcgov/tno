import { IDataSourceModel } from 'hooks/api-editor';
import React from 'react';
import { useNavigate } from 'react-router-dom';
import { useDataSources } from 'store/hooks/admin/sources';
import { useApp } from 'store/hooks/app/useApp';
import { GridTable } from 'tno-core/dist/components/grid-table';

import { DataSourceFilter } from '.';
import { dataSourceColumns } from './constants';
import * as styled from './styled';

interface IDataSourceListProps {}

export const DataSourceList: React.FC<IDataSourceListProps> = (props) => {
  const navigate = useNavigate();
  const [{ dataSources }, api] = useDataSources();
  const [{ requests }] = useApp();

  const [items, setItems] = React.useState<IDataSourceModel[]>([]);

  React.useEffect(() => {
    if (dataSources.length) {
      setItems(dataSources);
    } else {
      api.findAllDataSources().then((data) => {
        setItems(data);
      });
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  return (
    <styled.DataSourceList>
      <GridTable
        columns={dataSourceColumns}
        isLoading={!!requests.length}
        sorting={{ sortBy: [{ id: 'id', desc: false }] }}
        manualPageSize
        data={items}
        header={DataSourceFilter}
        onRowClick={(row) => navigate(`${row.original.id}`)}
      ></GridTable>
    </styled.DataSourceList>
  );
};
