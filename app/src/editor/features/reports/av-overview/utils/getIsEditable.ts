import { dayjs, type Dayjs } from '@/shared/utils/date';

export const getIsEditable = (publishedOn: Dayjs) => {
  const now = dayjs(Date.now());
  return (
    publishedOn.year() >= now.year() &&
    publishedOn.month() >= now.month() &&
    publishedOn.date() >= now.date()
  );
};
