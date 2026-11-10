import React from 'react';
import { IconButton, Row, Text } from 'tno-core';

import { useAdminStore } from '@/editor/store/slices';

interface IIngestTypeFilterProps {
  onFilterChange?: (value: string) => void;
}

export const IngestTypeFilter: React.FC<IIngestTypeFilterProps> = ({ onFilterChange }) => {
  const [{ ingestTypeFilter }, { storeIngestTypeFilter }] = useAdminStore();

  return (
    <Row className="filter-bar" justifyContent="center">
      <Text
        onChange={(e) => {
          storeIngestTypeFilter(e.target.value);
          onFilterChange?.(e.target.value);
        }}
        placeholder="Search by keyword"
        name="search"
        value={ingestTypeFilter}
      />
      <IconButton
        iconType="reset"
        onClick={() => {
          storeIngestTypeFilter('');
          onFilterChange?.('');
        }}
      />
    </Row>
  );
};
