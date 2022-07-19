import { FormPage } from 'components/form/formpage/styled';
import { IActionModel } from 'hooks/api-editor';
import React from 'react';
import { useNavigate } from 'react-router-dom';
import { useActions } from 'store/hooks/admin/actions';
import { useApp } from 'store/hooks/app/useApp';
import { GridTable } from 'tno-core';

import { CategoryListFilter } from './ActionListFilter';
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
        <GridTable
          columns={columns}
          header={CategoryListFilter}
          manualPageSize
          isLoading={!!requests.length}
          data={items}
          onRowClick={(row) => navigate(`${row.original.id}`)}
        ></GridTable>
      </FormPage>
    </styled.ActionList>
  );
};
