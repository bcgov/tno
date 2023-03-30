import React from 'react';
import { useNavigate } from 'react-router-dom';
import { useDataLocations } from 'store/hooks/admin';
import { useApp } from 'store/hooks/app/useApp';
import { Col, GridTable, IconButton, IDataLocationModel, Row } from 'tno-core';

import { columns } from './constants';
import { DataLocationListFilter } from './DataLocationListFilter';
import * as styled from './styled';

export const DataLocationList: React.FC = () => {
  const navigate = useNavigate();
  const [{ requests }] = useApp();
  const [{ dataLocations }, api] = useDataLocations();

  const [items, setItems] = React.useState<IDataLocationModel[]>([]);

  React.useEffect(() => {
    if (!dataLocations.length) {
      api.findAllDataLocations().then((data) => {
        setItems(data);
      });
    } else {
      setItems(dataLocations);
    }
  }, [api, dataLocations]);

  return (
    <styled.DataLocationList>
      <Row className="add-media" justifyContent="flex-end">
        <Col flex="1 1 0">
          DataLocations provide a way to identify different physical locations that data is stored.
          This provides a way for services to access remote files.
        </Col>
        <IconButton
          iconType="plus"
          label={`Add new data location`}
          onClick={() => navigate(`/admin/data/locations/0`)}
        />
      </Row>
      <GridTable
        columns={columns}
        header={DataLocationListFilter}
        paging={{ pageSizeOptions: { fromLocalStorage: true } }}
        isLoading={!!requests.length}
        data={items}
        onRowClick={(row) => navigate(`${row.original.id}`)}
      ></GridTable>
    </styled.DataLocationList>
  );
};
