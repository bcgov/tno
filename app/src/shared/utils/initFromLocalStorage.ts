import { getFromLocalStorage } from '.';

/**
 * Initialize values from local storage, or from the specified action.
 * If the action returns a new value it will be stored in local storage.
 * @param name The key to identify the value in local storage.
 * @param action An action to perform with the values from local storage.
 * @param saveResults Whether to save the results of the action to local storage.
 * @returns The value returned from the action.
 */
export const initFromLocalStorage = async <T>(
  name: string,
  action: (value?: T) => Promise<T>,
  saveResults: boolean = true,
) => {
  const value = getFromLocalStorage<T | undefined>(name, undefined);
  const result = await action(value);
  if (saveResults && value !== result) localStorage.setItem(name, JSON.stringify(result));
  return result;
};
