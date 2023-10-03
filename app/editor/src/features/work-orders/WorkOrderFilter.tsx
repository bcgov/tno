import React from 'react';
import { Checkbox, IconButton, IWorkOrderFilter, Row, Text, WorkOrderStatusName } from 'tno-core';

interface IWorkOrderFilterProps {
  filter: IWorkOrderFilter;
  onFilterChange: (filter: IWorkOrderFilter) => void;
}

export const WorkOrderFilter = ({ filter, onFilterChange }: IWorkOrderFilterProps) => {
  const [keywords, setKeywords] = React.useState<string>('');

  return (
    <Row className="filter" justifyContent="center" alignItems="center" gap="1rem">
      <Checkbox
        name="ready"
        label="Ready for review"
        checked={filter.status === WorkOrderStatusName.Completed}
        onChange={(e) =>
          onFilterChange({
            ...filter,
            status: e.target.checked ? WorkOrderStatusName.Completed : undefined,
          })
        }
      />
      <Text
        name="search"
        placeholder="Search by keyword"
        onChange={(e) => {
          setKeywords(e.target.value);
        }}
        onKeyDown={(e) => {
          if (e.code === 'Enter') onFilterChange((e.target as any).value);
        }}
        value={keywords}
      >
        <Row gap="0.25rem" nowrap>
          <IconButton
            iconType="search"
            title="Search"
            onClick={() => {
              onFilterChange({ ...filter, keywords });
            }}
          />
          <IconButton
            iconType="reset"
            title="Clear"
            onClick={() => {
              setKeywords('');
              onFilterChange({ ...filter, keywords: undefined });
            }}
          />
        </Row>
      </Text>
    </Row>
  );
};
