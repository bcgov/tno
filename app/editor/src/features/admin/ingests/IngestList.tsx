import React from 'react';
import { useNavigate } from 'react-router-dom';
import { useIngests } from 'store/hooks/admin';
import { useAdminStore } from 'store/slices';
import { Col, FlexboxTable, IconButton, Row } from 'tno-core';

import { columns } from './constants';
import { IngestFilter } from './IngestFilter';
import * as styled from './styled';
import { getStatus } from './utils';

interface IIngestListProps {}

const IngestList: React.FC<IIngestListProps> = (props) => {
  const navigate = useNavigate();
  const [{ ingests }, { findAllIngests, updateIngest }] = useIngests();
  const [{ ingestFilter }] = useAdminStore();

  const items = ingests.filter(
    (i) =>
      i.name.toLocaleLowerCase().includes(ingestFilter) ||
      i.source?.code.toLocaleLowerCase().includes(ingestFilter) ||
      i.description.toLocaleLowerCase().includes(ingestFilter) ||
      i.ingestType?.name.toLocaleLowerCase().includes(ingestFilter) ||
      getStatus(i).toLocaleLowerCase().includes(ingestFilter),
  );

  React.useEffect(() => {
    findAllIngests().catch(() => {});
    // Only init list.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  return (
    <styled.IngestList>
      <Row justifyContent="flex-end">
        <Col flex="1 1 0">
          Manage all ingest services. These services run in the background and upload content from
          external data sources.
        </Col>
        <IconButton
          iconType="plus"
          label="Add New Ingest"
          onClick={() => navigate('/admin/ingests/0')}
        />
      </Row>
      <IngestFilter />
      <FlexboxTable
        rowId="id"
        data={items}
        columns={columns}
        showSort={true}
        // onRowClick={(row) => navigate(`${row.original.id}`)}
        onCellClick={async (cell, row) => {
          if (cell.index !== 6) navigate(`${row.original.id}`);
          else {
            await updateIngest({ ...row.original, isEnabled: !row.original.isEnabled }).catch(
              () => {},
            );
          }
        }}
        pagingEnabled={false}
      />
    </styled.IngestList>
  );
};

export default IngestList;
