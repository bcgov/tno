import { FormPage } from 'components/form/formpage/styled';
import React from 'react';
import { useNavigate } from 'react-router-dom';
import { useUsers } from 'store/hooks/admin';
import { useApp } from 'store/hooks/app/useApp';
import { Page, PagedTable } from 'tno-core';

import { columns } from './constants';
import * as styled from './styled';
import { UserFilter } from './UserFilter';

export const UserList: React.FC = () => {
  const navigate = useNavigate();
  const [{ users }, api] = useUsers();
  const [{ requests }] = useApp();

  const [page, setPage] = React.useState(
    new Page(users.page - 1, users.quantity, users.items, users.total),
  );

  React.useEffect(() => {
    if (!users.items.length) {
      api.findUsers({ isSystemAccount: false }).then((data) => {
        setPage(new Page(data.page - 1, data.quantity, data.items, data.total));
      });
    }
  }, [api, users]);

  return (
    <styled.UserList>
      <FormPage>
        <PagedTable
          columns={columns}
          header={UserFilter}
          isLoading={!!requests.length}
          page={page}
          onRowClick={(row) => navigate(`${row.original.id}`)}
        ></PagedTable>
      </FormPage>
    </styled.UserList>
  );
};
