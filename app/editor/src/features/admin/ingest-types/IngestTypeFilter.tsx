import { IconButton } from 'components/form';
import React from 'react';
import { Row, Text } from 'tno-core';

interface IIngestTypeFilterProps {
  setGlobalFilter: (filterValue: any) => void;
}

export const IngestTypeFilter: React.FC<IIngestTypeFilterProps> = ({ setGlobalFilter }) => {
  const [filter, setFilter] = React.useState<string>('');
  return (
    <Row className="filter-bar" justifyContent="center">
      <Text
        onChange={(e) => {
          setFilter(e.target.value);
          setGlobalFilter(e.target.value);
        }}
        placeholder="Search by keyword"
        name="search"
        value={filter}
      />
      <IconButton
        iconType="reset"
        onClick={() => {
          setFilter('');
          setGlobalFilter('');
        }}
      />
    </Row>
  );
};
