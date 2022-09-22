import { IconButton } from 'components/form';
import React from 'react';
import { Row, Text } from 'tno-core';

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
