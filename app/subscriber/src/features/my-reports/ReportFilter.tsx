import { Button } from 'components/button';
import React from 'react';
import { FaFilter } from 'react-icons/fa6';
import { useProfileStore } from 'store/slices';
import { Row, Text } from 'tno-core';

/**
 * Provides simple filter component for reports.
 * @returns Component
 */
export const ReportFilter: React.FC = () => {
  const [{ reportsFilter }, { storeReportsFilter }] = useProfileStore();

  const [filter, setFilter] = React.useState(reportsFilter);

  return (
    <Row gap="1rem" flex="2">
      <Text
        label="Find Report:"
        name="filter"
        className="txt-filter"
        value={filter}
        onKeyDown={(e) => e.code === 'Enter' && storeReportsFilter(filter)}
        onChange={(e) => setFilter(e.target.value)}
      >
        <Button onClick={() => storeReportsFilter(filter)}>
          Filter
          <FaFilter />
        </Button>
      </Text>
    </Row>
  );
};
