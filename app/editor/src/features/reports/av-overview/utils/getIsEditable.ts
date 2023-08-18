import moment, { Moment } from 'moment';

export const getIsEditable = (publishedOn: Moment) => {
  const now = moment(Date.now());
  return (
    publishedOn.year() >= now.year() &&
    publishedOn.month() >= now.month() &&
    publishedOn.date() >= now.date()
  );
};
