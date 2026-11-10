import React from 'react';
import { IconButton, Text } from 'tno-core';
import { Row } from 'tno-core/dist/components/flex';

import * as styled from './styled';

export interface ISystemMessageFilterProps {
  onSearch?: (keywords: string) => void;
}

/**
 * Provides a component to filter system messages.
 * @returns Component
 */
export const SystemMessageFilter: React.FC<ISystemMessageFilterProps> = ({ onSearch }) => {
  const [filter, setFilter] = React.useState<string>('');

  return (
    <styled.SystemMessageFilter>
      <Row className="filter-bar" justifyContent="center">
        <Text
          onChange={(e) => {
            setFilter(e.target.value);
          }}
          placeholder="Search by keyword"
          name="keyword"
          value={filter}
          onKeyDown={(event) => {
            if (event.key === 'Enter') {
              event.preventDefault();
              onSearch?.(filter);
            }
          }}
        />
        <IconButton
          iconType="search"
          onClick={() => {
            onSearch?.(filter);
          }}
        />
        <IconButton
          iconType="reset"
          onClick={() => {
            setFilter('');
            onSearch?.('');
          }}
        />
      </Row>
    </styled.SystemMessageFilter>
  );
};
