import React from 'react';
import { Checkbox, IconButton, Row, Text } from 'tno-core';

interface IAdminFilterProps {
  notificationId: number;
  onFilterChange?: (value: string) => void;
  onSearch?: (value: string, isSubscribedToNotificationId: number | undefined) => void;
}

export const NotificationFilter: React.FC<IAdminFilterProps> = ({
  notificationId,
  onFilterChange,
  onSearch,
}) => {
  const [filter, setFilter] = React.useState<string>('');
  const [isSubscribed, setIsSubscribed] = React.useState<number>();
  return (
    <Row className="filter-bar" justifyContent="center">
      <Text
        onChange={(e) => {
          setFilter(e.target.value);
          onFilterChange?.(e.target.value);
        }}
        onKeyUp={(e) => {
          if (e.code === 'Enter') onSearch?.(filter, isSubscribed);
        }}
        placeholder="Search by keyword"
        name="search"
        value={filter}
      >
        {!!onSearch && (
          <IconButton
            iconType="search"
            onClick={() => {
              onSearch?.(filter, isSubscribed);
            }}
          />
        )}
      </Text>
      <Checkbox
        name="isSubscribed"
        label="Is subscribed"
        onChange={(e) => {
          const s = e.target.checked ? notificationId : undefined;
          setIsSubscribed(s);
          onSearch?.(filter, s);
        }}
      />
      <IconButton
        iconType="reset"
        onClick={() => {
          setFilter('');
          onFilterChange?.('');
          onSearch?.('', undefined);
        }}
      />
    </Row>
  );
};
