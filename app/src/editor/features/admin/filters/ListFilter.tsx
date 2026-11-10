import React from 'react';
import { IconButton, Row, Text } from 'tno-core';

import { useAdminStore } from '@/editor/store/slices';

interface IAdminFilterProps {
  onFilterChange?: (value: string) => void;
  onSearch?: (value: string) => void;
}

export const ListFilter: React.FC<IAdminFilterProps> = ({ onFilterChange, onSearch }) => {
  const [{ filterFilter }, { storeFilterFilter }] = useAdminStore();

  return (
    <Row
      className="filter-bar field-row"
      justifyContent="center"
      alignItems="flex-end"
      gap="0.5rem"
    >
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
      />
      {!!onSearch && (
        <IconButton
          iconType="search"
          title="Search"
          onClick={() => {
            onSearch?.(filterFilter);
          }}
        />
      )}
      <IconButton
        iconType="reset"
        title="Clear the search"
        onClick={() => {
          storeFilterFilter('');
          onFilterChange?.('');
          onSearch?.('');
        }}
      />
    </Row>
  );
};
