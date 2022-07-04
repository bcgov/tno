import { FormPage } from 'components/form/formpage/styled';
import { makeUserFilter } from 'features/content/list-view/utils/makeUserFilter';
import { IUserFilter, IUserModel } from 'hooks';
import React from 'react';
import { useNavigate } from 'react-router-dom';
import { SortingRule } from 'react-table';
import { useUsers } from 'store/hooks/admin';
import { useApp } from 'store/hooks/app/useApp';
import { Page, PagedTable } from 'tno-core';

import { columns } from './constants';
import * as styled from './styled';
import { UserFilter } from './UserFilter';

export const UserList: React.FC = () => {
  const navigate = useNavigate();
  const [{ users, userFilter }, { findUsers, storeFilter }] = useUsers();
  const [{ requests }, { isUserReady }] = useApp();
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

  const fetch = React.useCallback(
    async (filter: IUserFilter) => {
      try {
        const data = await findUsers(makeUserFilter(filter));
        console.log(data);
        const page = new Page(data.page - 1, data.quantity, data?.items, data.total);

        setPage(page);
        // return page;
      } catch (error) {
        // TODO: Handle error
        throw error;
      }
    },
    [findUsers],
  );

  React.useEffect(() => {
    // Only make a request if the user has been set.
    if (isUserReady()) {
      fetch(userFilter);
    }
    // Only want to make a request when filter or sort change.
    // 'fetch' regrettably changes any time the advanced filter changes.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [userFilter, isUserReady()]);

  return (
    <styled.UserList>
      <FormPage>
        <PagedTable
          columns={columns}
          header={UserFilter}
          sorting={{ sortBy: userFilter.sort }}
          isLoading={!!requests.length}
          page={page}
          onRowClick={(row) => navigate(`${row.original.id}`)}
          onChangeSort={handleChangeSort}
        ></PagedTable>
      </FormPage>
    </styled.UserList>
  );
};
