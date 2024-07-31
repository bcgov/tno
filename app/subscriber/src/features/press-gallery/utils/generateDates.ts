import moment from 'moment';

/** function that generates dates for the  last 7 days */
export const generateDates = () => {
  const dates: any = Array.from({ length: 7 }, (_, i) => {
    const date = moment().subtract(i, 'days');
    return {
      label: date.format('MMMM DD - YYYY'),
      value: date.format(),
    };
  });
  return dates;
};
