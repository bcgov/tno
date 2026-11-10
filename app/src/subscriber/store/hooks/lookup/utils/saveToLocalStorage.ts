/**
 * Save the specified value into local storage.
 * @param name The key to identify the value in local storage.
 * @param value The value to save to local storage.
 * @param action An action to perform with the values.
 * @returns The value returned from the action.
 */
export const saveToLocalStorage = <T>(name: string, value: T, action: (value: T) => void) => {
  localStorage.setItem(name, JSON.stringify(value));
  action(value);
};
