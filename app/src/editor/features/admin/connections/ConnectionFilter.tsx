import React from 'react';
import { IconButton, Row, Text } from 'tno-core';

interface IAdminFilterProps {
  onFilterChange: (value: string) => void;
}

/**
 * Filter for connections.
 * @param param0 Component properties.
 * @returns Component.
 */
export const ConnectionFilter: React.FC<IAdminFilterProps> = ({ onFilterChange }) => {
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
