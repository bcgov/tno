import React from 'react';
import { useNavigate } from 'react-router-dom';
import { useApp } from 'store/hooks';
import { useMinisters } from 'store/hooks/admin';
import { Col, FlexboxTable, IconButton, IMinisterModel, Row } from 'tno-core';

import { columns } from './constants';
import { MinisterFilter } from './MinisterFilter';
import * as styled from './styled';

/**
 * Admin list view for ministers.
 * @returns Component.
 */
const MinisterList: React.FC = () => {
  const navigate = useNavigate();
  const [{ ministers }, api] = useMinisters();

  const [items, setItems] = React.useState<IMinisterModel[]>([]);
  const [{ requests }] = useApp();

  React.useEffect(() => {
    // setIsLoading(true);
    if (!ministers.length) {
      api.findAllMinisters().then((data) => {
        setItems(data);
        // setIsLoading(false);
      });
    } else {
      setItems(ministers);
      // setIsLoading(false);
    }
  }, [api, ministers]);

  return (
    <styled.MinisterList>
      <Row className="add-media" justifyContent="flex-end">
        <Col flex="1 1 0">
          Ministers provide a way to configuration data storage locations and authentication
          settings. Ingest service configuration requires both a source and destination minister to
          be configured.
        </Col>
        <IconButton
          iconType="plus"
          label={`Add new minister`}
          onClick={() => navigate(`/admin/ministers/0`)}
        />
      </Row>
      <MinisterFilter
        onFilterChange={(filter) => {
          if (filter && filter.length) {
            const value = filter.toLocaleLowerCase();
            setItems(
              ministers.filter(
                (i) =>
                  i.name.toLocaleLowerCase().includes(value) ||
                  i.description.toLocaleLowerCase().includes(value),
              ),
            );
          } else {
            setItems(ministers);
          }
        }}
      />
      <FlexboxTable
        rowId="id"
        data={items}
        columns={columns}
        showSort={true}
        onRowClick={(row) => navigate(`${row.original.id}`)}
        pagingEnabled={false}
        isLoading={!!requests.length}
      />
    </styled.MinisterList>
  );
};

export default MinisterList;
