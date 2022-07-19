import { IconButton } from 'components/form';
import React from 'react';
import { useNavigate } from 'react-router-dom';
import { Row, Text } from 'tno-core';

interface IAdminFilterProps {
  setGlobalFilter: (filterValue: any) => void;
}

export const CategoryListFilter: React.FC<IAdminFilterProps> = ({ setGlobalFilter }) => {
  const [filter, setFilter] = React.useState<string>('');
  const navigate = useNavigate();
  return (
    <>
      <Row className="add-media" justifyContent="flex-end">
        <IconButton
          iconType="plus"
          label={`Add new action`}
          onClick={() => navigate(`/admin/actions/0`)}
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
