import { makeUserFilter } from 'features/admin/users/utils/makeUserFilter';
import React from 'react';
import { useNavigate } from 'react-router-dom';
import { useUsers } from 'store/hooks/admin';
import {
  Col,
  FlexboxTable,
  IconButton,
  ITablePage,
  ITableSort,
  IUserModel,
  Page,
  Row,
} from 'tno-core';

import { columns } from './constants';
import { IUserListFilter } from './interfaces/IUserListFilter';
import * as styled from './styled';
import { UserFilter } from './UserFilter';

const UserList: React.FC = () => {
  const navigate = useNavigate();
  const [{ users, userFilter }, { findUsers, storeFilter }] = useUsers();

  const [filter, setFilter] = React.useState<IUserListFilter>();
  const [page, setPage] = React.useState(
    new Page(users.page - 1, users.quantity, users.items, users.total),
  );

  const handleChangeSort = React.useCallback(
    (sort: ITableSort<IUserModel>[]) => {
      const sorts = sort.filter((s) => s.isSorted).map((s) => ({ id: s.id, desc: s.isSortedDesc }));
      storeFilter({ ...userFilter, sort: sorts });
    },
    [storeFilter, userFilter],
  );

  const handleChangePage = React.useCallback(
    (page: ITablePage) => {
      if (userFilter.pageIndex !== page.pageIndex || userFilter.pageSize !== page.pageSize) {
        storeFilter({
          ...userFilter,
          pageIndex: page.pageIndex,
          pageSize: page.pageSize ?? userFilter.pageSize,
        });
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
      } catch {}
    },
    [findUsers],
  );

  React.useEffect(() => {
    if (filter !== userFilter) {
      setFilter(userFilter); // Need this to stop infinite loop.
      fetch(userFilter);
    }
  }, [fetch, filter, userFilter]);

  return (
    <styled.UserList>
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
      <UserFilter />
      <FlexboxTable
        rowId="id"
        data={page.items}
        columns={columns}
        showSort={true}
        manualPaging={true}
        pageIndex={page.pageIndex}
        pageSize={page.pageSize}
        onRowClick={(row) => navigate(`${row.original.id}`)}
        onPageChange={handleChangePage}
        onSortChange={handleChangeSort}
      />
    </styled.UserList>
  );
};

export default UserList;
