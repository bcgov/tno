import React from 'react';
import { useAdminStore } from 'store/slices';
import { IconButton, Row, Text } from 'tno-core';

interface IAdminFilterProps {
  onFilterChange?: (value: string) => void;
  onSearch?: (value: string) => void;
}

export const ListFilter: React.FC<IAdminFilterProps> = ({ onFilterChange, onSearch }) => {
  const [{ filterFilter }, { storeFilterFilter }] = useAdminStore();

  return (
    <Row className="filter-bar" justifyContent="center">
      <Text
        onChange={(e) => {
          storeFilterFilter(e.target.value);
          onFilterChange?.(e.target.value);
        }}
        onKeyUp={(e) => {
          if (e.code === 'Enter') onSearch?.(filterFilter);
        }}
        placeholder="Search by keyword"
        name="search"
        value={filterFilter}
      >
        {!!onSearch && (
          <IconButton
            iconType="search"
            onClick={() => {
              onSearch?.(filterFilter);
            }}
          />
        )}
      </Text>
      <IconButton
        iconType="reset"
        onClick={() => {
          storeFilterFilter('');
          onFilterChange?.('');
          onSearch?.('');
        }}
      />
    </Row>
  );
};
