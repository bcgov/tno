import { FormPage, IconButton } from 'components/form';
import { ISourceModel } from 'hooks/api-editor';
import React from 'react';
import { useNavigate } from 'react-router-dom';
import { useSources } from 'store/hooks/admin';
import { useApp } from 'store/hooks/app/useApp';
import { Col, Row } from 'tno-core';
import { GridTable } from 'tno-core/dist/components/grid-table';

import { SourceFilter } from '.';
import { columns } from './constants';
import * as styled from './styled';

interface ISourceListProps {}

export const SourceList: React.FC<ISourceListProps> = (props) => {
  const navigate = useNavigate();
  const [{ sources }, api] = useSources();
  const [{ requests }] = useApp();

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
      <GridTable
        columns={columns}
        isLoading={!!requests.length}
        sorting={{ sortBy: [{ id: 'id', desc: false }] }}
        manualPageSize
        data={items}
        header={SourceFilter}
        onRowClick={(row) => navigate(`${row.original.id}`)}
      ></GridTable>
    </styled.SourceList>
  );
};
