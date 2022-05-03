/**
 * Update the value in the array, or add it to the array if it doesn't exist.
 * @param value
 * @param values
 * @param find
 * @returns
 */
export const addOrUpdate = <T>(value: T, values: T[], find: (value: T) => boolean) => {
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
