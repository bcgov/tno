import React from 'react';
import { IconButton, Row, Text } from 'tno-core';

interface IAdminFilterProps {
  onFilterChange?: (value: string) => void;
  onSearch?: (value: string) => void;
}

export const ProductFilter: React.FC<IAdminFilterProps> = ({ onFilterChange, onSearch }) => {
  const [filter, setFilter] = React.useState<string>('');
  return (
    <Row className="filter-bar" justifyContent="center">
      <Text
        onChange={(e) => {
          setFilter(e.target.value);
          onFilterChange?.(e.target.value);
        }}
        onKeyUp={(e) => {
          if (e.code === 'Enter') onSearch?.(filter);
        }}
        placeholder="Search by keyword"
        name="search"
        value={filter}
      >
        {!!onSearch && (
          <IconButton
            iconType="search"
            onClick={() => {
              onSearch?.(filter);
            }}
          />
        )}
      </Text>
      <IconButton
        iconType="reset"
        onClick={() => {
          setFilter('');
          onFilterChange?.('');
          onSearch?.('');
        }}
      />
    </Row>
  );
};
