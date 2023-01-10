import React from 'react';
import { IconButton, Row, Text } from 'tno-core';

interface IIngestFilterProps {
  setGlobalFilter: (filterValue: any) => void;
}

export const IngestFilter: React.FC<IIngestFilterProps> = ({ setGlobalFilter }) => {
  const [filter, setFilter] = React.useState<string>('');
  return (
    <Row className="filter" justifyContent="center">
      <Text
        name="search"
        placeholder="Search by keyword"
        onChange={(e) => {
          setFilter(e.target.value);
          setGlobalFilter(e.target.value);
        }}
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
