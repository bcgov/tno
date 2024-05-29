import React from 'react';
import { FaCaretDown, FaCaretUp, FaFilter } from 'react-icons/fa6';

import { SortDirection } from './constants';

interface ISortActionProps {
  direction?: SortDirection;
  onChange?: (direction: SortDirection) => void;
}

export const SortAction = ({ direction = SortDirection.None, onChange }: ISortActionProps) => {
  const [value, setValue] = React.useState<SortDirection>(direction);

  React.useEffect(() => {
    setValue(direction);
  }, [direction]);

  return value === SortDirection.None ? (
    <FaFilter
      title="sort"
      onClick={() => {
        setValue(SortDirection.Ascending);
        onChange?.(SortDirection.Ascending);
      }}
    />
  ) : value === SortDirection.Ascending ? (
    <FaCaretUp
      title="ascending"
      onClick={() => {
        setValue(SortDirection.Descending);
        onChange?.(SortDirection.Descending);
      }}
    />
  ) : (
    <FaCaretDown
      title="descending"
      onClick={() => {
        setValue(SortDirection.None);
        onChange?.(SortDirection.None);
      }}
    />
  );
};
