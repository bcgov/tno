import moment from 'moment';

/** Format a run timestamp for display. */
export const formatRunTime = (value?: string): string =>
  value ? moment(value).format('YYYY-MM-DD HH:mm:ss') : '-';
