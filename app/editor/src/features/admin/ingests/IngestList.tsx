import React from 'react';
import { useNavigate } from 'react-router-dom';
import { useIngests } from 'store/hooks/admin';
import { useApp } from 'store/hooks/app/useApp';
import { Col, IconButton, IIngestModel, Row } from 'tno-core';
import { GridTable } from 'tno-core/dist/components/grid-table';

import { IngestFilter } from '.';
import { columns } from './constants';
import * as styled from './styled';

interface IIngestListProps {}

export const IngestList: React.FC<IIngestListProps> = (props) => {
  const navigate = useNavigate();
  const [{ ingests }, api] = useIngests();
  const [{ requests }] = useApp();

  const [items, setItems] = React.useState<IIngestModel[]>([]);

  React.useEffect(() => {
    if (ingests.length) {
      setItems(ingests);
    } else {
      api.findAllIngests().then((data) => {
        setItems(data);
      });
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  return (
    <styled.IngestList>
      <Row justifyContent="flex-end">
        <Col flex="1 1 0">
          Ingest management provides a way to configure ingestion services. These services run in
          the background and upload content from external data sources.
        </Col>
        <IconButton
          iconType="plus"
          label="Add New Ingest"
          onClick={() => navigate('/admin/ingests/0')}
        />
      </Row>
      <GridTable
        columns={columns}
        isLoading={!!requests.length}
        sorting={{ sortBy: [{ id: 'id', desc: false }] }}
        manualPageSize
        data={items}
        header={IngestFilter}
        onRowClick={(row) => navigate(`${row.original.id}`)}
      ></GridTable>
    </styled.IngestList>
  );
};
