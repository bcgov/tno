import React from 'react';
import { useNavigate } from 'react-router-dom';
import { useSources } from 'store/hooks/admin';
import { Col, FlexboxTable, IconButton, ISourceModel, Row } from 'tno-core';

import { columns } from './constants';
import * as styled from './styled';

interface ISourceListProps {}

export const SourceList: React.FC<ISourceListProps> = (props) => {
  const navigate = useNavigate();
  const [{ sources }, api] = useSources();

  const [items, setItems] = React.useState<ISourceModel[]>([]);

  React.useEffect(() => {
    if (sources.length) {
      setItems(sources);
    } else {
      api.findAllSources().then((data) => {
        setItems(data);
      });
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  return (
    <styled.SourceList>
      <Row justifyContent="flex-end">
        <Col flex="1 1 0">
          Sources provide a way to identify the source of the content. Generally this would be the
          publisher, or channel.
        </Col>
        <IconButton
          iconType="plus"
          label="Add New Source"
          onClick={() => navigate('/admin/sources/0')}
        />
      </Row>
      <FlexboxTable
        rowId="id"
        data={items}
        columns={columns}
        showSort={true}
        onRowClick={(row) => navigate(`${row.original.id}`)}
      />
    </styled.SourceList>
  );
};
