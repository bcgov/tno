import { IconButton } from 'components/form';
import React from 'react';
import { Row, Text } from 'tno-core';

interface IAdminFilterProps {
  setGlobalFilter: (filterValue: any) => void;
}

/**
 * Filter for connections.
 * @param param0 Component properties.
 * @returns Component.
 */
export const ConnectionListFilter: React.FC<IAdminFilterProps> = ({ setGlobalFilter }) => {
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
