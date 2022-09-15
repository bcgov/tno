import { FormPage, IconButton } from 'components/form';
import { makeUserFilter } from 'features/content/list-view/utils/makeUserFilter';
import { IUserModel } from 'hooks';
import React from 'react';
import { useNavigate } from 'react-router-dom';
import { SortingRule } from 'react-table';
import { useUsers } from 'store/hooks/admin';
import { useApp } from 'store/hooks/app/useApp';
import { Col, Page, PagedTable, Row } from 'tno-core';

import { columns } from './constants';
import { IUserListFilter } from './interfaces/IUserListFilter';
import * as styled from './styled';
import { UserFilter } from './UserFilter';

export const UserList: React.FC = () => {
  const navigate = useNavigate();
  const [{ users, userFilter }, { findUsers, storeFilter }] = useUsers();
  const [{ requests }] = useApp();
  const [page, setPage] = React.useState(
    new Page(users.page - 1, users.quantity, users.items, users.total),
  );

  const handleChangeSort = React.useCallback(
    (sortBy: SortingRule<IUserModel>[]) => {
      const sorts = sortBy.map((sb) => ({ id: sb.id, desc: sb.desc }));
      const same = sorts.every(
        (val, i) => val.id === userFilter.sort[i]?.id && val.desc === userFilter.sort[i]?.desc,
      );
      if (!same) {
        storeFilter({ ...userFilter, sort: sorts });
      }
    },
    [storeFilter, userFilter],
  );

  const handleChangePage = React.useCallback(
    (pi: number, ps?: number) => {
      console.log(pi, ps);
      if (userFilter.pageIndex !== pi || userFilter.pageSize !== ps) {
        console.log('here');
        storeFilter({ ...userFilter, pageIndex: pi, pageSize: ps ?? userFilter.pageSize });
      }
    },
    [userFilter, storeFilter],
  );

  const fetch = React.useCallback(
    async (filter: IUserListFilter) => {
      try {
        const data = await findUsers(makeUserFilter(filter));
        const page = new Page(data.page - 1, data.quantity, data?.items, data.total);

        setPage(page);
        return page;
      } catch (error) {
        // TODO: Handle error
        throw error;
      }
    },
    [findUsers],
  );

  React.useEffect(() => {
    fetch(userFilter);
  }, [fetch, userFilter]);

  return (
    <styled.UserList>
      <FormPage>
        <Row className="add-media" justifyContent="flex-end">
          <Col flex="1 1 0">
            User administration provides a way to approve users and grant them appropriate roles.
          </Col>
          <IconButton
            iconType="plus"
            label="Add New User"
            onClick={() => navigate('/admin/users/0')}
          />
        </Row>
        <PagedTable
          columns={columns}
          header={UserFilter}
          sorting={{ sortBy: userFilter.sort }}
          isLoading={!!requests.length}
          page={page}
          onRowClick={(row) => navigate(`${row.original.id}`)}
          onChangeSort={handleChangeSort}
          onChangePage={handleChangePage}
        ></PagedTable>
      </FormPage>
    </styled.UserList>
  );
};
