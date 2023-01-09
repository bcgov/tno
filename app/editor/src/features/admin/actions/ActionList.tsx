import { IActionModel } from 'hooks/api-editor';
import React from 'react';
import { useNavigate } from 'react-router-dom';
import { useActions } from 'store/hooks/admin';
import { useApp } from 'store/hooks/app/useApp';
import { Col, FormPage, GridTable, IconButton, Row } from 'tno-core';

import { ActionListFilter } from './ActionListFilter';
import { columns } from './constants';
import * as styled from './styled';

export const ActionList: React.FC = () => {
  const navigate = useNavigate();
  const [{ requests }] = useApp();
  const [{ actions }, api] = useActions();

  const [items, setItems] = React.useState<IActionModel[]>([]);

  React.useEffect(() => {
    if (!actions.length) {
      api.findAllActions().then((data) => {
        setItems(data);
      });
    } else {
      setItems(actions);
    }
  }, [api, actions]);

  return (
    <styled.ActionList>
      <FormPage>
        <Row className="add-media" justifyContent="flex-end">
          <Col flex="1 1 0">
            Actions provide a way for administrators to identify actions that should be performed on
            content, or which reports content should be included in.
          </Col>
          <IconButton
            iconType="plus"
            label={`Add new action`}
            onClick={() => navigate(`/admin/actions/0`)}
          />
        </Row>
        <GridTable
          columns={columns}
          header={ActionListFilter}
          manualPageSize
          isLoading={!!requests.length}
          data={items}
          onRowClick={(row) => navigate(`${row.original.id}`)}
        ></GridTable>
      </FormPage>
    </styled.ActionList>
  );
};
