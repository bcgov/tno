import { dayjs } from '@/shared/utils/date';

export const calcDuration = (option: number) => {
  switch (option) {
    case 0:
      return {
        start: dayjs().add(-7, 'days').toDate(),
        end: null,
      };
    case 1:
      return {
        start: dayjs().add(-14, 'days').toDate(),
        end: null,
      };
    case 2: // Current Month
      return {
        start: dayjs().startOf('month').toDate(),
        end: dayjs().endOf('month').toDate(),
      };
    case 3: // Previous Month
      return {
        start: dayjs().add(-1, 'months').startOf('month').toDate(),
        end: dayjs().add(-1, 'months').endOf('month').toDate(),
      };
    default:
      return {
        start: null,
        end: null,
      };
  }
};
