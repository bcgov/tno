import moment from 'moment';
/**
 * @param {string | Date} date - The date to format
 * @param {boolean} includeTime - Whether to include the time in the formatted date
 * @returns {string} - The formatted date
 * @description - This function takes a date and returns a formatted date string
 */
export const formatDate = (date: string | Date | undefined, includeTime?: boolean): string => {
  if (date === undefined) return '';
  const result = moment(date);
  if (!result.isValid()) return '';
  if (includeTime) {
    return result.format('MMMM DD, YYYY HH:mm:ss');
  } else {
    return result.format('MMMM DD - YYYY');
  }
};
