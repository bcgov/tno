import { Row } from 'components/flex/row';
import { Text } from 'components/form';
import React from 'react';

interface IDataSourceFilterProps {
  setGlobalFilter: (filterValue: any) => void;
}

export const DataSourceFilter: React.FC<IDataSourceFilterProps> = ({ setGlobalFilter }) => {
  return (
    <Row className="filter">
      <Text
        name="search"
        label="Search"
        onChange={(e) => {
          setGlobalFilter(e.target.value);
        }}
      />
    </Row>
  );
};
