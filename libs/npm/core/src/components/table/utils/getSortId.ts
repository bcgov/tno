import { ITableHookColumn } from '../interfaces';

export const getSortId = <T extends object>(col: ITableHookColumn<T>, index: number) => {
  return typeof col.accessor === 'function'
    ? `col-${index}`
    : col.accessor?.toString() ?? `col-${index}`;
};
