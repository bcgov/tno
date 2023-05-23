import React from 'react';
import { IconButton, Row, Text } from 'tno-core';

interface IIngestFilterProps {
  onFilterChange: (value: string) => void;
}

export const IngestFilter = ({ onFilterChange }: IIngestFilterProps) => {
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
