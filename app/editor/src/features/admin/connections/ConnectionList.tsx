import { FormPage, IconButton } from 'components/form';
import { IConnectionModel } from 'hooks/api-editor';
import React from 'react';
import { useNavigate } from 'react-router-dom';
import { useConnections } from 'store/hooks/admin/connections';
import { useApp } from 'store/hooks/app/useApp';
import { Col, GridTable, Row } from 'tno-core';

import { ConnectionListFilter } from './ConnectionListFilter';
import { columns } from './constants';
import * as styled from './styled';

export const ConnectionList: React.FC = () => {
  const navigate = useNavigate();
  const [{ requests }] = useApp();
  const [{ connections }, api] = useConnections();

  const [items, setItems] = React.useState<IConnectionModel[]>([]);

  React.useEffect(() => {
    if (!connections.length) {
      api.findAllConnections().then((data) => {
        setItems(data);
      });
    } else {
      setItems(connections);
    }
  }, [api, connections]);

  return (
    <styled.ConnectionList>
      <FormPage>
        <Row className="add-media" justifyContent="flex-end">
          <Col flex="1 1 0">
            Connections provide a way to configuration data storage locations and authentication
            settings. Ingest service configuration requires both a source and destination connection
            to be configured.
          </Col>
          <IconButton
            iconType="plus"
            label={`Add new connection`}
            onClick={() => navigate(`/admin/connections/0`)}
          />
        </Row>
        <GridTable
          columns={columns}
          header={ConnectionListFilter}
          manualPageSize
          isLoading={!!requests.length}
          data={items}
          onRowClick={(row) => navigate(`${row.original.id}`)}
        ></GridTable>
      </FormPage>
    </styled.ConnectionList>
  );
};
