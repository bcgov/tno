import { FormPage } from 'components/formpage';
import React from 'react';
import { useNavigate } from 'react-router-dom';
import { useActions } from 'store/hooks/admin';
import { Col, FlexboxTable, IActionModel, IconButton, Row } from 'tno-core';

import { ActionFilter } from './ActionFilter';
import { columns } from './constants';
import * as styled from './styled';

const ActionList: React.FC = () => {
  const navigate = useNavigate();
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
        <ActionFilter
          onFilterChange={(filter) => {
            if (filter && filter.length) {
              const value = filter.toLocaleLowerCase();
              setItems(
                actions.filter(
                  (i) =>
                    i.name.toLocaleLowerCase().includes(value) ||
                    i.description.toLocaleLowerCase().includes(value),
                ),
              );
            } else {
              setItems(actions);
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
        />
      </FormPage>
    </styled.ActionList>
  );
};

export default ActionList;
