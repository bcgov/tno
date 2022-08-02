import { IconButton, OptionItem, Select } from 'components/form';
import { UserStatus, UserStatusName } from 'hooks';
import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useUsers } from 'store/hooks/admin';
import { FieldSize, Text } from 'tno-core';
import { Row } from 'tno-core/dist/components/flex';

import { IUserListFilter } from './interfaces/IUserListFilter';

interface IUserFilterProps {}

export const UserFilter: React.FC<IUserFilterProps> = () => {
  const [{ userFilter }, { storeFilter }] = useUsers();
  const [filter, setFilter] = useState<IUserListFilter>(userFilter);
  const statusOptions = [
    new OptionItem(UserStatusName.Preapproved, UserStatus.Preapproved),
    new OptionItem(UserStatusName.Approved, UserStatus.Approved),
    new OptionItem(UserStatusName.Activated, UserStatus.Activated),
    new OptionItem(UserStatusName.Authenticated, UserStatus.Authenticated),
    new OptionItem(UserStatusName.Denied, UserStatus.Denied),
    new OptionItem(UserStatusName.Requested, UserStatus.Requested),
  ];

  const navigate = useNavigate();
  return (
    <>
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
          value={statusOptions.find((s) => s.value === Number(filter.status)) || ''}
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
    </>
  );
};
