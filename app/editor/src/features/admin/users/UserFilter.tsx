import React from 'react';
import { useLookup } from 'store/hooks';
import { useUsers } from 'store/hooks/admin';
import {
  FieldSize,
  filterEnabledOptions,
  getEnumStringOptions,
  IconButton,
  OptionItem,
  Select,
  Text,
  UserAccountTypeName,
  UserStatusName,
} from 'tno-core';
import { Row } from 'tno-core/dist/components/flex';

import { IUserListFilter } from './interfaces/IUserListFilter';
import * as styled from './styled';

interface IUserFilterProps {}

/**
 * Provides a component to filter users.
 * @returns Component
 */
export const UserFilter: React.FC<IUserFilterProps> = () => {
  const [{ userFilter }, { storeFilter }] = useUsers();
  const [lookups] = useLookup();

  const [filter, setFilter] = React.useState<IUserListFilter>({ ...userFilter, keyword: '' });
  const [roleOptions, setRoleOptions] = React.useState(
    lookups.roles.map((r) => new OptionItem(r.name, r.id, !r.isEnabled)),
  );

  const statusOptions = getEnumStringOptions(UserStatusName);
  const accountTypeOptions = getEnumStringOptions(UserAccountTypeName);

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
            if (e) setFilter({ ...filter, keyword: e.target.value });
            else setFilter({ ...filter, keyword: undefined });
          }}
          placeholder="Search by keyword"
          name="keyword"
          value={filter.keyword}
        />
        <Select
          onChange={(e: any) => {
            if (e) setFilter({ ...filter, accountType: e.value });
            else setFilter({ ...filter, accountType: undefined });
          }}
          width={FieldSize.Medium}
          options={accountTypeOptions}
          name="accountType"
          placeholder="Search by Type"
          value={accountTypeOptions.find((s) => s.value === filter.accountType) || ''}
        />
        <Select
          onChange={(e: any) => {
            if (e) setFilter({ ...filter, status: e.value });
            else setFilter({ ...filter, status: undefined });
          }}
          width={FieldSize.Medium}
          options={statusOptions}
          name="status"
          placeholder="Search by status"
          value={statusOptions.find((s) => s.value === filter.status) || ''}
        />
        <Select
          onChange={(e: any) => {
            if (e) setFilter({ ...filter, roleName: e.value });
            else setFilter({ ...filter, roleName: undefined });
          }}
          width={FieldSize.Medium}
          options={filterEnabledOptions(roleOptions, filter.roleName)}
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
              page: 0,
              quantity: 20,
            });
            storeFilter({ sort: [], page: 0, quantity: 20 });
          }}
        />
      </Row>
    </styled.UserFilter>
  );
};
