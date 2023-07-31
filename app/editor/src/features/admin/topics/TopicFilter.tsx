import React from 'react';
import { IconButton, Row, Text } from 'tno-core';

interface IAdminFilterProps {
  onFilterChange: (value: string) => void;
}

export const TopicFilter: React.FC<IAdminFilterProps> = ({ onFilterChange }) => {
  const [filter, setFilter] = React.useState<string>('');
  return (
    <div className="topic-filter">
      <Row>
        <label>
          <b>Existing topics</b>
        </label>
      </Row>
      <Row className="filter-bar">
        <Text
          onChange={(e) => {
            setFilter(e.target.value);
            onFilterChange(e.target.value);
          }}
          placeholder="Search by keyword"
          name="search"
          value={filter}
        />
        <IconButton
          iconType="reset"
          onClick={() => {
            setFilter('');
            onFilterChange('');
          }}
        />
      </Row>
    </div>
  );
};
