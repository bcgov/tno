import React from 'react';
import { IconButton, Row, Text } from 'tno-core';

import { useAdminStore } from '@/editor/store/slices';

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
