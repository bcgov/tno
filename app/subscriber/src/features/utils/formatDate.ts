import moment from 'moment';
/**
 * @param {string | Date} date - The date to format
 * @param {boolean} includeTime - Whether to include the time in the formatted date
 * @returns {string} - The formatted date
 * @description - This function takes a date and returns a formatted date string
 */
export const formatDate = (date: string | Date, includeTime?: boolean) => {
  if (includeTime) {
    return moment(date).format('MMMM DD, YYYY hh:mm:ss');
  } else {
    return moment(date).format('MMMM DD, YYYY');
  }
};
