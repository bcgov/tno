/**
 * Calculate the size and return a formatted descriptive string.
 * @param bytes
 * @param descriptor
 * @returns
 */
export const calcSize = (bytes?: number, descriptor: string = 'MB') => {
  var value = '';
  if (!!bytes)
    value = (bytes / 1024 / 1024).toLocaleString(undefined, { maximumFractionDigits: 2 });

  if (descriptor === undefined || !value) descriptor = '';

  return `${value}${descriptor}`;
};
