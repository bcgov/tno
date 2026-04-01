import React from 'react';
import { IconButton, Row, Text } from 'tno-core';

interface ILLMFilterProps {
  onFilterChange: (value: string) => void;
}

export const LLMFilter: React.FC<ILLMFilterProps> = ({ onFilterChange }) => {
  const [filter, setFilter] = React.useState<string>('');
  return (
    <Row className="filter-bar" justifyContent="center">
      <Text
        onChange={(e) => {
          setFilter(e.target.value);
          onFilterChange(e.target.value);
        }}
        placeholder="Search by keyword"
        name="search"
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
