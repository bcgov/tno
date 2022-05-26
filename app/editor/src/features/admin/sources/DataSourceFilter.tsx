import { IconButton, Text } from 'components/form';
import React from 'react';
import { useNavigate } from 'react-router-dom';
import { Row } from 'tno-core/dist/components/flex/row';

interface IDataSourceFilterProps {
  setGlobalFilter: (filterValue: any) => void;
}

export const DataSourceFilter: React.FC<IDataSourceFilterProps> = ({ setGlobalFilter }) => {
  const [filter, setFilter] = React.useState<string>('');
  const navigate = useNavigate();
  return (
    <>
      <Row justifyContent="flex-end">
        <IconButton
          iconType="plus"
          label="Add New Data Source"
          onClick={() => navigate('/admin/data/sources/0')}
        />
      </Row>
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
    </>
  );
};
