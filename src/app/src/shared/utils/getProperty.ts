/**
 * Get the property at the specified 'path' for the specified 'item'.
 * @param item Object to parse property from.
 * @param path Path to property on the object to get (i.e. path.to.property, or path.to.array.2).
 * @returns Property value at the specified path.
 */
export const getProperty = <T extends object>(item: T, path?: string | keyof T) => {
  const paths = typeof path === 'string' ? path.split('.') : path;
  if (!paths) return item;
  var prop: any = { ...item };
  if (Array.isArray(paths)) {
    while (paths.length && prop && (prop = prop[paths.shift() ?? '']));
    return prop;
  } else {
    return prop[paths];
  }
};
