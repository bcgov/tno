import { IconButton } from 'components/form';
import React from 'react';
import { Row, Text } from 'tno-core';

interface ISourceFilterProps {
  setGlobalFilter: (filterValue: any) => void;
}

export const SourceFilter: React.FC<ISourceFilterProps> = ({ setGlobalFilter }) => {
  const [filter, setFilter] = React.useState<string>('');
  return (
    <Row className="filter" justifyContent="center">
      <Text
        name="search"
        placeholder="Search by keyword"
        onChange={(e) => {
          setFilter(e.target.value);
          setGlobalFilter(e.target.value);
        }}
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
  );
};
