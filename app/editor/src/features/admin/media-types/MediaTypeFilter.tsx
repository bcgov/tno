import { IconButton } from 'components/form';
import React from 'react';
import { useNavigate } from 'react-router-dom';
import { Row, Text } from 'tno-core';

interface IMediaTypeFilterProps {
  setGlobalFilter: (filterValue: any) => void;
}

export const MediaTypeFilter: React.FC<IMediaTypeFilterProps> = ({ setGlobalFilter }) => {
  const [filter, setFilter] = React.useState<string>('');
  const navigate = useNavigate();
  return (
    <>
      <Row className="add-media" justifyContent="flex-end">
        <IconButton
          iconType="plus"
          label="Add New Media Type"
          onClick={() => navigate('/admin/media/types/0')}
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
