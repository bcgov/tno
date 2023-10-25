import React from 'react';
import { useNavigate } from 'react-router-dom';
import { useIngestTypes } from 'store/hooks/admin';
import { useAdminStore } from 'store/slices';
import { Col, FlexboxTable, IconButton, IIngestTypeModel, Row } from 'tno-core';

import { columns } from './constants';
import { IngestTypeFilter } from './IngestTypeFilter';
import * as styled from './styled';

const IngestTypeList: React.FC = () => {
  const navigate = useNavigate();
  const [{ ingestTypes }, { findAllIngestTypes }] = useIngestTypes();
  const [{ ingestTypeFilter }] = useAdminStore();

  const [items, setItems] = React.useState<IIngestTypeModel[]>(ingestTypes);

  React.useEffect(() => {
    if (!ingestTypes.length) {
      findAllIngestTypes().catch(() => {});
    }
    // Only on init
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  React.useEffect(() => {
    if (ingestTypeFilter && ingestTypeFilter.length) {
      const value = ingestTypeFilter.toLocaleLowerCase();
      setItems(
        ingestTypes.filter(
          (i) =>
            i.name.toLocaleLowerCase().includes(value) ||
            i.description.toLocaleLowerCase().includes(value),
        ),
      );
    } else {
      setItems(ingestTypes);
    }
  }, [ingestTypes, ingestTypeFilter]);

  return (
    <styled.IngestTypeList>
      <Row className="add-ingest" justifyContent="flex-end">
        <Col flex="1 1 0">
          Ingest types provide a way to identify the ingest that the content represents. Each ingest
          service is configured to listen to one or more of these ingest types.
        </Col>
        <IconButton
          iconType="plus"
          label="Add New Ingest Type"
          onClick={() => navigate('/admin/ingest/types/0')}
        />
      </Row>
      <IngestTypeFilter />
      <FlexboxTable
        rowId="id"
        data={items}
        columns={columns}
        showSort={true}
        onRowClick={(row) => navigate(`${row.original.id}`)}
        pagingEnabled={false}
      />
    </styled.IngestTypeList>
  );
};

export default IngestTypeList;
