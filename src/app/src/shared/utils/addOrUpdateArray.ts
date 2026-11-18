/**
 * Update the value in the array, or add it to the array if it doesn't exist.
 * This function does not mutate the supplied array.
 * @param value The value to add or update to the array.
 * @param values The current array.
 * @param find Predicate to find the item in the array if it exists.
 * @returns A new array containing the value.
 */
export const addOrUpdateArray = <T>(value: T, values: T[], find: (value: T) => boolean) => {
  let found = false;
  const result = values.map((v) => {
    if (find(v)) {
      found = true;
      return value;
    }
    return v;
  });

  if (!found) result.push(value);
  return result;
};
