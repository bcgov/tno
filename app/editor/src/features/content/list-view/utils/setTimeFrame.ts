import moment from 'moment';

/**
 * Function used to help filter content by time frame.
 * @param {number} timeFrame - Time frame passed down from the filter
 * @returns Time frame coupled to today, the last 24 hours, or the last 48 hours.
 */
export const setTimeFrame = (timeFrame: number) => {
  if (timeFrame === 0) return moment().startOf('day').toDate();
  else if (timeFrame === 1) return moment().add(-24, 'hours').toDate();
  else if (timeFrame === 2) return moment().add(-48, 'hours').toDate();
  return undefined;
};
