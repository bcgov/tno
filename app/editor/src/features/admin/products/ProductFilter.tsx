import React from 'react';
import { Checkbox, IconButton, IUserFilter, Row, Show, Text } from 'tno-core';

interface IAdminFilterProps {
  productId?: number;
  filter: IUserFilter;
  onChange?: (filter: IUserFilter) => void;
  onSearch?: (filter: IUserFilter) => void;
}

export const ProductFilter: React.FC<IAdminFilterProps> = ({
  productId,
  filter,
  onChange,
  onSearch,
}) => {
  const [keywords, setKeywords] = React.useState('');

  return (
    <Row className="filter-bar" justifyContent="center">
      <Show visible={productId !== undefined}>
        <Checkbox
          name="isSubscribed"
          label="Is subscribed"
          className="checkbox-filter"
          checked={!!filter?.isSubscribedToProductId}
          onChange={(e) => {
            onSearch?.({
              ...filter,
              isSubscribedToProductId: e.target.checked ? productId : undefined,
            });
          }}
        />
      </Show>
      <Text
        onChange={(e) => {
          setKeywords(e.target.value);
          onChange?.({ ...filter, keyword: keywords });
        }}
        onKeyUp={(e) => {
          if (e.code === 'Enter') onSearch?.({ ...filter, keyword: keywords });
        }}
        placeholder="Search by keyword"
        name="search"
        value={keywords}
      >
        {!!onSearch && (
          <IconButton
            iconType="search"
            onClick={() => {
              onSearch?.({ ...filter, keyword: keywords });
            }}
          />
        )}
      </Text>
      <IconButton
        iconType="reset"
        onClick={() => {
          setKeywords('');
          onSearch?.({ ...filter, keyword: '', isSubscribedToProductId: undefined });
        }}
      />
    </Row>
  );
};
