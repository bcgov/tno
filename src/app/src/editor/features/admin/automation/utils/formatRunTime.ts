import { dayjs } from '@/shared/utils/date';

/** Format a run timestamp for display. */
export const formatRunTime = (value?: string): string =>
  value ? dayjs(value).format('YYYY-MM-DD HH:mm:ss') : '-';
