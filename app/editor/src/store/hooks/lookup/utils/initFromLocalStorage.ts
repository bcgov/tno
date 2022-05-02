import { getFromLocalStorage } from '.';

/**
 * Initialize values from local storage, or from the specified action.
 * If the action returns a new value it will be stored in local storage.
 * @param name The key to identify the value in local storage.
 * @param defaultValue The default value if local storage doesn't contain a value.
 * @param action An action to perform with the values from local storage.
 * @returns The value returned from the action.
 */
export const initFromLocalStorage = async <T>(
  name: string,
  defaultValue: T,
  action: (value: T) => Promise<T>,
) => {
  const value = getFromLocalStorage<T>(name, defaultValue);
  const result = await action(value);
  if (value !== result) localStorage.setItem(name, JSON.stringify(result));
  return result;
};
