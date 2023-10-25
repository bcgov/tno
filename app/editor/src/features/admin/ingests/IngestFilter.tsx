import React from 'react';
import { useAdminStore } from 'store/slices';
import { IconButton, Row, Text } from 'tno-core';

interface IIngestFilterProps {
  onFilterChange?: (value: string) => void;
}

export const IngestFilter = ({ onFilterChange }: IIngestFilterProps) => {
  const [{ ingestFilter }, { storeIngestFilter }] = useAdminStore();

  return (
    <Row className="filter" justifyContent="center">
      <Text
        name="search"
        placeholder="Search by keyword"
        onChange={(e) => {
          storeIngestFilter(e.target.value);
          onFilterChange?.(e.target.value);
        }}
        value={ingestFilter}
      />
      <IconButton
        iconType="reset"
        onClick={() => {
          storeIngestFilter('');
          onFilterChange?.('');
        }}
      />
    </Row>
  );
};
