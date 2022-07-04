import { IconButton, OptionItem, Select } from 'components/form';
import { IUserFilter, UserStatus, UserStatusName } from 'hooks';
import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useUsers } from 'store/hooks/admin';
import { FieldSize, Text } from 'tno-core';
import { Row } from 'tno-core/dist/components/flex';

interface IUserFilterProps {}

export const UserFilter: React.FC<IUserFilterProps> = () => {
  const [{ userFilter }, { storeFilter }] = useUsers();
  const [filter, setFilter] = useState<IUserFilter>(userFilter);
  const statusOptions = [
    new OptionItem<number>(UserStatusName.Preapproved, UserStatus.Preapproved),
    new OptionItem<number>(UserStatusName.Approved, UserStatus.Approved),
    new OptionItem<number>(UserStatusName.Activated, UserStatus.Activated),
    new OptionItem<number>(UserStatusName.Authenticated, UserStatus.Authenticated),
    new OptionItem<number>(UserStatusName.Denied, UserStatus.Denied),
    new OptionItem<number>(UserStatusName.Requested, UserStatus.Requested),
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
        />
        <Select
          onChange={(e: any) => {
            setFilter({ ...filter, status: e.value });
          }}
          width={FieldSize.Medium}
          options={statusOptions}
          name="status"
          placeholder="Search by status"
        />
        <Text
          onChange={(e) => {
            setFilter({ ...filter, role: e.target.value });
          }}
          name="role"
          placeholder="Search by role"
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
            setFilter({ sort: [] });
            storeFilter({ sort: [] });
          }}
        />
      </Row>
    </>
  );
};
