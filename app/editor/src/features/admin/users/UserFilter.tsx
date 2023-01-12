import { UserStatusName } from 'hooks';
import React, { useState } from 'react';
import { useLookup } from 'store/hooks';
import { useUsers } from 'store/hooks/admin';
import { FieldSize, IconButton, OptionItem, Select, Text } from 'tno-core';
import { Row } from 'tno-core/dist/components/flex';
import { getEnumStringOptions } from 'utils';

import { IUserListFilter } from './interfaces/IUserListFilter';
import * as styled from './styled';

interface IUserFilterProps {}

export const UserFilter: React.FC<IUserFilterProps> = () => {
  const [{ userFilter }, { storeFilter }] = useUsers();
  const [filter, setFilter] = useState<IUserListFilter>({ ...userFilter, keyword: '' });
  const statusOptions = getEnumStringOptions(UserStatusName);
  const [lookups] = useLookup();
  const [roleOptions, setRoleOptions] = React.useState(
    lookups.roles.map((r) => new OptionItem(r.name, r.id)),
  );

  React.useEffect(() => {
    setRoleOptions(lookups.roles.map((r) => new OptionItem(r.name, r.id)));
  }, [lookups.roles]);

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
          options={roleOptions}
          name="role"
          placeholder="Search by role"
          value={roleOptions.find((s) => s.value === filter.roleName) || ''}
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
              pageSize: 20,
            });
            storeFilter({ sort: [], pageIndex: 0, pageSize: 20 });
          }}
        />
      </Row>
    </styled.UserFilter>
  );
};
