import { getFilename } from './getFilename';

/**
 * Parses path and remove any filename.
 * @param path Path with or without filename.
 * @returns The full page without the filename.
 */
export const getDirectoryName = (path?: string) => {
  const filename = getFilename(path);
  const directory = filename?.includes('.') ? path?.replace(filename, '') : path;
  return directory?.replace(/[\\/]$/, '');
};
