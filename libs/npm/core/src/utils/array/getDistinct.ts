/**
 * Generates a new array of distinct items based on the specified 'predicate'.
 * @param values An array of values.
 * @param predicate Predicate to extract value from item in array.
 * @returns A new array with distinct values based on the predicate.
 */
export const getDistinct = (values: any[], predicate: (item: any) => any) => {
  return values.filter((a, i) => values.findIndex((s) => predicate(a) === predicate(s)) === i);
};
