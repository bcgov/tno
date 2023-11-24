import { ISortableModel } from '../../hooks/api';

export const sortable = <IdT extends string | number, ST extends ISortableModel<IdT>>(
  a: ST,
  b: ST,
) => {
  if (a.sortOrder < b.sortOrder) return -1;
  else if (a.sortOrder > b.sortOrder) return 1;
  return 0;
};
