import { ITableInternalHeaderColumn } from '../interfaces/ITableInternalHeaderColumn';

export const determineSortValue = <T extends object>(col: ITableInternalHeaderColumn<T>) => {
  // If 'col.sort' is defined and not null, use it directly for sorting
  if (col.sort !== undefined && col.sort !== null) {
    return col.sort;
  }
  // If 'col.sort' is undefined or null and 'accessor' is a function,
  // it indicates that 'accessor' cannot be used for sorting directly (e.g., dynamic values)
  if (typeof col.accessor === 'function') {
    return undefined;
  }
  // If 'col.sort' is undefined or null, and 'accessor' is not a function,
  // use 'accessor' as the field name for sorting
  return col.accessor;
};
