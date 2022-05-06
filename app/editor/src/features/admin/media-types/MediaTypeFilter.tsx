import { IconButton, Text } from 'components/form';
import React from 'react';
import { useNavigate } from 'react-router-dom';
import { Row } from 'tno-core/dist/components/flex';

import { NEW_MEDIA_TYPE_ROUTE } from './constants';

interface IMediaTypeFilterProps {
  setGlobalFilter: (filterValue: any) => void;
}

export const MediaTypeFilter: React.FC<IMediaTypeFilterProps> = ({ setGlobalFilter }) => {
  const [filter, setFilter] = React.useState<string>('');
  const navigate = useNavigate();
  return (
    <>
      <Row className="add-media" justify="flex-end">
        <IconButton
          iconType="plus"
          label="Add New Media Type"
          onClick={() => navigate(NEW_MEDIA_TYPE_ROUTE)}
        />
      </Row>
      <Row className="filter-bar" justify="center">
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
