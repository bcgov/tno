import { IconButton } from 'components/form';
import React from 'react';
import { useNavigate } from 'react-router-dom';
import { Row, Text } from 'tno-core';

interface IClipDirectoryFilterProps {
  setGlobalFilter: (filterValue: any) => void;
}

export const ClipDirectoryFilter: React.FC<IClipDirectoryFilterProps> = ({ setGlobalFilter }) => {
  const [filter, setFilter] = React.useState<string>('');
  const navigate = useNavigate();
  return (
    <>
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
