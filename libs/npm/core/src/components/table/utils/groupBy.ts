import { ITableGroup } from '../interfaces/ITableGroup';

/**
 * Group an array by the specified 'predicate'.
 * @param array An array of objects.
 * @param predicate A predicate to generate the grouping.
 * @returns A new array grouped by the predicate.
 */
export const groupBy = <T>(
  array: T[],
  predicate: (item: T, index: number, array: T[]) => string,
): ITableGroup<T>[] => {
  const grouping = array.reduce((acc, value, index, array) => {
    let key = predicate(value, index, array);
    if (key === undefined || key === null) key = '_';
    else key = `_${key}`;

    (acc[key] ||= []).push(value);
    return acc;
  }, {} as { [key: string]: T[] });

  return Object.keys(grouping).map((key) => ({
    key: key.substring(1),
    rows: grouping[key],
  }));
};

export type TableRowGroup<T> = T & {
  /** A unique key to identify what group a row belongs to. */
  _key: string;
};

/**
 * Group an array by the specified 'predicate'.
 * Flattens the results by adding the group key to each row of the original array items.
 * @param array An array of objects.
 * @param predicate A predicate to generate the grouping.
 * @returns A new array grouped by the predicate.
 */
export const flattenGroupBy = <T>(
  array: T[],
  predicate: (item: T, index: number, array: T[]) => string,
): TableRowGroup<T>[] => {
  const flatten: TableRowGroup<T>[] = [];

  groupBy(array, predicate).forEach((group) => {
    const rows = group.rows.map((row) => ({ ...row, _key: group.key }));
    flatten.push.apply(flatten, rows);
  });

  return flatten;
};
