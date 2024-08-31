import { SortDirection } from 'tno-core';

export const sortData = (aValue: any, bValue: any, direction: SortDirection) => {
  if (typeof aValue === 'boolean' && typeof bValue === 'boolean') {
    if (aValue === bValue) return 0;
    if (aValue) {
      return direction === SortDirection.Ascending ? -1 : 1;
    } else {
      return direction === SortDirection.Ascending ? 1 : -1;
    }
  } else {
    if (aValue > bValue) return direction === SortDirection.Ascending ? 1 : -1;
    if (aValue < bValue) return direction === SortDirection.Ascending ? -1 : 1;
    return 0;
  }
};
