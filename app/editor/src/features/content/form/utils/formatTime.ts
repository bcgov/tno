/**
 * Function used to format time to a commonly used format.
 * @param {string} date The date to format.
 * @returns The date in ms since epoch, used to convert date to another date object.
 */
export const formatTime = (date: string) => {
  const converted = new Date(date);
  return !!converted.getTime() ? converted.getTime() : '';
};
