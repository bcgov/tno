import { IconButton, Select } from 'components/form';
import { UserRoleName, UserStatusName } from 'hooks';
import React, { useState } from 'react';
import { useUsers } from 'store/hooks/admin';
import { FieldSize, Text } from 'tno-core';
import { Row } from 'tno-core/dist/components/flex';
import { getEnumStringOptions } from 'utils';

import { IUserListFilter } from './interfaces/IUserListFilter';
import * as styled from './styled';

interface IUserFilterProps {}

export const UserFilter: React.FC<IUserFilterProps> = () => {
  const [{ userFilter }, { storeFilter }] = useUsers();
  const [filter, setFilter] = useState<IUserListFilter>(userFilter);
  const statusOptions = getEnumStringOptions(UserStatusName);
  const roles = getEnumStringOptions(UserRoleName);

  /** Handle enter key pressed for user filter */
  React.useEffect(() => {
    const keyDownHandler = (event: KeyboardEvent) => {
      if (event.key === 'Enter') {
        event.preventDefault();
        storeFilter(filter);
      }
    };

    document.addEventListener('keydown', keyDownHandler);

    return () => {
      document.removeEventListener('keydown', keyDownHandler);
    };
  }, [filter, storeFilter]);

  return (
    <styled.UserFilter>
      <Row className="filter-bar" justifyContent="center">
        <Text
          onChange={(e) => {
            setFilter({ ...filter, keyword: e.target.value });
          }}
          placeholder="Search by keyword"
          name="keyword"
          value={filter.keyword}
        />
        <Select
          onChange={(e: any) => {
            setFilter({ ...filter, status: e.value });
          }}
          width={FieldSize.Medium}
          options={statusOptions}
          name="status"
          placeholder="Search by status"
          value={statusOptions.find((s) => s.value === filter.status) || ''}
        />
        <Select
          onChange={(e: any) => {
            setFilter({ ...filter, roleName: e.value });
          }}
          width={FieldSize.Medium}
          options={roles}
          name="role"
          placeholder="Search by role"
          value={roles.find((s) => s.value === filter.roleName) || ''}
        />
        <IconButton
          iconType="search"
          onClick={() => {
            storeFilter(filter);
          }}
        />
        <IconButton
          iconType="reset"
          onClick={() => {
            setFilter({
              sort: [],
              roleName: undefined,
              keyword: '',
              status: undefined,
              pageIndex: 0,
              pageSize: 10,
            });
            storeFilter({ sort: [], pageIndex: 0, pageSize: 10 });
          }}
        />
      </Row>
    </styled.UserFilter>
  );
};
