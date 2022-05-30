import { IconButton, Text } from 'components/form';
import React from 'react';
import { useNavigate } from 'react-router-dom';
import { Row } from 'tno-core/dist/components/flex';

interface IUserFilterProps {
  setGlobalFilter: (filterValue: any) => void;
}

export const UserFilter: React.FC<IUserFilterProps> = ({ setGlobalFilter }) => {
  const [filter, setFilter] = React.useState<string>('');
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
            setFilter(e.target.value);
            setGlobalFilter(e.target.value);
          }}
          placeholder="Search by keyword"
          name="search"
          value={filter}
        />
        <IconButton
          iconType="reset"
          onClick={() => {
            setFilter('');
            setGlobalFilter('');
          }}
        />
      </Row>
    </>
  );
};
