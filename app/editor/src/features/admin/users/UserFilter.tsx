import { IconButton, Select } from 'components/form';
import { UserStatusName } from 'hooks';
import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
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

  const navigate = useNavigate();

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
      <Row className="add-media" justifyContent="flex-end">
        <IconButton
          iconType="plus"
          label="Add New User"
          onClick={() => navigate('/admin/users/0')}
        />
      </Row>
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
        <Text
          onChange={(e) => {
            setFilter({ ...filter, roleName: e.target.value });
          }}
          name="role"
          placeholder="Search by role"
          value={filter.roleName}
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
            setFilter({ sort: [], roleName: '', keyword: '', status: undefined });
            storeFilter({ sort: [] });
          }}
        />
      </Row>
    </styled.UserFilter>
  );
};
