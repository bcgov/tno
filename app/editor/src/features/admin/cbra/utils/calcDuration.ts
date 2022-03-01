import moment from 'moment';

export const calcDuration = (option: number) => {
  switch (option) {
    case 0:
      return {
        start: moment().add(-7, 'days').toDate(),
        end: null,
      };
    case 1:
      return {
        start: moment().add(-14, 'days').toDate(),
        end: null,
      };
    case 2: // Current Month
      return {
        start: moment().startOf('month').toDate(),
        end: moment().endOf('month').toDate(),
      };
    case 3: // Previous Month
      return {
        start: moment().add(-1, 'months').startOf('month').toDate(),
        end: moment().add(-1, 'months').endOf('month').toDate(),
      };
    default:
      return {
        start: null,
        end: null,
      };
  }
};
