import React from 'react';
import { IconButton, Row, Text } from 'tno-core';

interface ISourceFilterProps {
  onFilterChange: (value: string) => void;
}

export const SourceFilter: React.FC<ISourceFilterProps> = ({ onFilterChange }) => {
  const [filter, setFilter] = React.useState<string>('');
  return (
    <Row className="filter" justifyContent="center">
      <Text
        name="search"
        placeholder="Search by keyword"
        onChange={(e) => {
          setFilter(e.target.value);
          onFilterChange(e.target.value);
        }}
        value={filter}
      />
      <IconButton
        iconType="reset"
        onClick={() => {
          setFilter('');
          onFilterChange('');
        }}
      />
    </Row>
  );
};
