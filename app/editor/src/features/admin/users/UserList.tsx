import { makeUserFilter } from 'features/admin/users/utils/makeUserFilter';
import _ from 'lodash';
import React from 'react';
import { useNavigate } from 'react-router-dom';
import { useUsers } from 'store/hooks/admin';
import {
  CellCheckbox,
  CellDate,
  CellEllipsis,
  Col,
  Grid,
  IconButton,
  IGridHeaderColumnProps,
  IUserModel,
  Page,
  Row,
  SortDirection,
} from 'tno-core';

import { IUserListFilter } from './interfaces/IUserListFilter';
import * as styled from './styled';
import { UserFilter } from './UserFilter';

const UserList: React.FC = () => {
  const navigate = useNavigate();
  const [{ users, userFilter }, { findUsers, storeFilter }] = useUsers();

  const [filter, setFilter] = React.useState<IUserListFilter | null>(null);
  const [page, setPage] = React.useState(
    new Page(users.page - 1, users.quantity, users.items, users.total),
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
    if (filter === null || !_.isEqual(filter, userFilter)) {
      setFilter(userFilter); // Need this to stop infinite loop.
      fetch(userFilter);
    }
  }, [fetch, filter, userFilter]);

  const handleSortChange = React.useCallback(
    (column: IGridHeaderColumnProps, direction: SortDirection) => {
      if (column.name) {
        const sort =
          direction === SortDirection.None
            ? []
            : [{ id: column.name, desc: direction === SortDirection.Descending }];
        storeFilter({ ...userFilter, sort });
      }
    },
    [userFilter, storeFilter],
  );

  const handleQuantityChange = React.useCallback(
    (quantity: number) => {
      if (userFilter.quantity !== quantity) {
        const newFilter = {
          ...userFilter,
          quantity: quantity,
        };
        storeFilter(newFilter);
      }
    },
    [userFilter, storeFilter],
  );

  const handlePageChange = React.useCallback(
    (page: number) => {
      if (userFilter.page !== page - 1) {
        const newFilter = {
          ...userFilter,
          page: page - 1,
        };
        storeFilter(newFilter);
      }
    },
    [userFilter, storeFilter],
  );

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
      <Grid
        items={page.items}
        pageIndex={page.pageIndex}
        itemsPerPage={page.pageSize}
        totalItems={page.total}
        showPaging
        onNavigatePage={async (page) => {
          handlePageChange(page);
        }}
        onQuantityChange={async (quantity) => {
          handleQuantityChange(quantity);
        }}
        onSortChange={async (column, direction) => {
          handleSortChange(column, direction);
        }}
        renderHeader={() => [
          {
            name: 'username',
            label: 'Username',
            size: '10%',
            sortable: true,
          },
          { name: 'email', label: 'Email', size: '20%', sortable: true },
          { name: 'lastName', label: 'Last Name', size: '14%', sortable: true },
          { name: 'firstName', label: 'First Name', size: '14%', sortable: true },
          { name: 'roles', label: 'Role(s)', size: '15%', sortable: true },
          { name: 'lastLogin', label: 'Last Login', sortable: true },
          { name: 'isEnabled', label: 'Enabled', size: '7%', sortable: true },
          { name: 'status', label: 'Status', size: '8%', sortable: true },
        ]}
        renderColumns={(row: IUserModel, rowIndex) => {
          return [
            {
              column: (
                <div key="" className="clickable" onClick={() => navigate(`${row.id}`)}>
                  <CellEllipsis key="">{row.username}</CellEllipsis>
                </div>
              ),
            },
            {
              column: (
                <div key="" className="clickable" onClick={() => navigate(`${row.id}`)}>
                  <CellEllipsis key="">{row.email}</CellEllipsis>
                </div>
              ),
            },
            {
              column: (
                <div key="" className="clickable" onClick={() => navigate(`${row.id}`)}>
                  <CellEllipsis key="">{row.lastName}</CellEllipsis>
                </div>
              ),
            },
            {
              column: (
                <div key="" className="clickable" onClick={() => navigate(`${row.id}`)}>
                  <CellEllipsis key="">{row.firstName}</CellEllipsis>
                </div>
              ),
            },
            {
              column: (
                <div key="" className="clickable" onClick={() => navigate(`${row.id}`)}>
                  <CellEllipsis key="">{row.roles?.join(', ')}</CellEllipsis>
                </div>
              ),
            },
            {
              column: (
                <div key="" className="clickable" onClick={() => navigate(`${row.id}`)}>
                  <CellDate value={row.lastLoginOn} />
                </div>
              ),
            },
            {
              column: (
                <div key="" className="clickable" onClick={() => navigate(`${row.id}`)}>
                  <CellCheckbox checked={row.isEnabled} />
                </div>
              ),
            },
            {
              column: (
                <div key="" className="clickable" onClick={() => navigate(`${row.id}`)}>
                  <CellEllipsis key="">{row.status}</CellEllipsis>
                </div>
              ),
            },
          ];
        }}
      />
    </styled.UserList>
  );
};

export default UserList;
