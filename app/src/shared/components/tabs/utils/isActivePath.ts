import { isRelativePath } from '.';

/**
 * Determine if the specified 'path' is the active path in the URL.
 * Please note that if the 'path' is relative and you don't specify 'exact', then it will only need to find the 'path' inside the 'location'.
 * This can lead to false positives.
 * @param location URL location path without query string.
 * @param path The path to look for to determine if it's active.
 * @param exact Whether the path must match exactly.
 * @returns True if the path is active.
 */
export const isActivePath = (location: string, path?: string, exact: boolean = true) => {
  if ((!path && (!!location || location === '/')) || location === path) return true;
  if (!path) return false;
  if (isRelativePath(path)) return exact ? location.endsWith(path) : location.includes(path);
  return exact ? location === path : location.startsWith(path);
};
