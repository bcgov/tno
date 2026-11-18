import { ISortableModel } from '../../hooks/api';

/**
 * Provides default sorting to sortable models.
 * > sortOrder, name, id
 * @param a Item one
 * @param b Item two
 * @returns Value to determine sort action.
 */
export const sortable = <IdT extends string | number, ST extends ISortableModel<IdT>>(
  a: ST,
  b: ST,
) => {
  if (a.sortOrder < b.sortOrder) return -1;
  else if (a.sortOrder > b.sortOrder) return 1;
  else if (a.name < b.name) return -1;
  else if (a.name > b.name) return 1;
  else if (a.id < b.id) return -1;
  else if (a.id > b.id) return 1;
  return 0;
};
