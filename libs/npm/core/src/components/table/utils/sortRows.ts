import { ITableInternalRow, ITableSort } from '..';

/**
 * Provides a way to sort the table rows with multiple sort rules.
 * Applies the rules in the order they are specified.
 * @param rows An array of rows.
 * @param sortOrder An array of sort rules.
 * @returns An array of sorted rows.
 */
export const sortRows = <T extends object>(
  rows: ITableInternalRow<T>[],
  sortOrder: ITableSort<T>[] = [],
) => {
  const result = rows
    .sort((a, b) => {
      // Find the first sort that is not equal.
      const sortBy = sortOrder.find((sort) => {
        if (!sort.isSorted) return false;

        if (typeof sort.sort === 'string') {
          const aVal = (a.original as any)[sort.sort];
          const bVal = (b.original as any)[sort.sort];

          if (aVal === bVal) return false;
          return true;
        }

        if (sort.sort(a) === sort.sort(b)) return false;
        return true;
      });

      // Apply the sort.
      if (sortBy) {
        if (typeof sortBy.sort === 'string') {
          const aVal = (a.original as any)[sortBy.sort] ?? '';
          const bVal = (b.original as any)[sortBy.sort] ?? '';

          const outcome = (aVal < bVal ? -1 : 1) * (sortBy.isSortedDesc ? -1 : 1);
          return outcome;
        }

        const aVal = sortBy.sort(a) ?? '';
        const bVal = sortBy.sort(b) ?? '';
        return (aVal < bVal ? -1 : 1) * (sortBy.isSortedDesc ? -1 : 1);
      }

      return 0;
    })
    .map((row, index) => {
      row.index = index;
      return row;
    });
  return result;
};
