import React from 'react';
import { useAdminStore } from 'store/slices';
import { IconButton, Row, Text } from 'tno-core';

interface IAdminFilterProps {
  onFilterChange?: (value: string) => void;
  onSearch?: (value: string) => void;
}

export const ListFilter: React.FC<IAdminFilterProps> = ({ onFilterChange, onSearch }) => {
  const [{ reportFilter }, { storeReportFilter }] = useAdminStore();

  return (
    <Row className="filter-bar" justifyContent="center">
      <Text
        onChange={(e) => {
          storeReportFilter(e.target.value);
          onFilterChange?.(e.target.value);
        }}
        onKeyUp={(e) => {
          if (e.code === 'Enter') onSearch?.(reportFilter);
        }}
        placeholder="Search by keyword"
        name="search"
        value={reportFilter}
      ></Text>
      {!!onSearch && (
        <IconButton
          iconType="search"
          onClick={() => {
            onSearch?.(reportFilter);
          }}
        />
      )}
      <IconButton
        iconType="reset"
        onClick={() => {
          storeReportFilter('');
          onFilterChange?.('');
          onSearch?.('');
        }}
      />
    </Row>
  );
};
