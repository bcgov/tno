import React from 'react';
import { IconButton, Row, Text } from 'tno-core';

interface IAdminFilterProps {
  setGlobalFilter: (filterValue: any) => void;
}

export const LicenseListFilter: React.FC<IAdminFilterProps> = ({ setGlobalFilter }) => {
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
