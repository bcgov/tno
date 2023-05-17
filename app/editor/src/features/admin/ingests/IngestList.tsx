import React from 'react';
import { useNavigate } from 'react-router-dom';
import { useIngests } from 'store/hooks/admin';
import { Col, FlexboxTable, IconButton, IIngestModel, Row } from 'tno-core';

import { columns } from './constants';
import { IngestFilter } from './IngestFilter';
import * as styled from './styled';

interface IIngestListProps {}

export const IngestList: React.FC<IIngestListProps> = (props) => {
  const navigate = useNavigate();
  const [{ ingests }, api] = useIngests();

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
      <IngestFilter
        setGlobalFilter={(filter) => {
          if (filter && filter.length) {
            const value = filter.toLocaleLowerCase();
            setItems(
              ingests.filter(
                (i) =>
                  i.name.toLocaleLowerCase().includes(value) ||
                  i.source?.code.toLocaleLowerCase().includes(value) ||
                  i.description.toLocaleLowerCase().includes(value) ||
                  i.ingestType?.name.toLocaleLowerCase().includes(value),
              ),
            );
          } else {
            setItems(ingests);
          }
        }}
      />
      <FlexboxTable
        rowId="id"
        data={items}
        columns={columns}
        showSort={true}
        onRowClick={(row) => navigate(`${row.original.id}`)}
      />
    </styled.IngestList>
  );
};
