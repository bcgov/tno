/**
 * Get an array of values from local storage if the supplied items are empty.
 * @param name The key to identify the value in local storage.
 * @param defaultValue The default value if local storage doesn't contain a value.
 * @returns The value from local storage or the default value.
 */
export const getFromLocalStorage = <T>(name: string, defaultValue: T) => {
  const value = localStorage.getItem(name);
  if (value === null) return defaultValue;
  return JSON.parse(value) as T;
};
