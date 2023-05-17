import React from 'react';
import { useNavigate } from 'react-router-dom';
import { useDataLocations } from 'store/hooks/admin';
import { Col, FlexboxTable, IconButton, IDataLocationModel, Row } from 'tno-core';

import { columns } from './constants';
import * as styled from './styled';

export const DataLocationList: React.FC = () => {
  const navigate = useNavigate();
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
      <FlexboxTable
        rowId="id"
        data={items}
        columns={columns}
        showSort={true}
        onRowClick={(row) => navigate(`${row.original.id}`)}
      />
    </styled.DataLocationList>
  );
};
