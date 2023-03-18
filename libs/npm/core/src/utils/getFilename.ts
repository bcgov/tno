/**
 * Parses path to find last segment name.
 * @param path Path with or without file.
 * @returns The last segment of a path.
 */
export const getFilename = (path?: string) => {
  return !!path ? decodeURIComponent(path).replace(/^.*[\\/]/, '') : path;
};
