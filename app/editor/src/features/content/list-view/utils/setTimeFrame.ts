import moment from 'moment';

export const setTimeFrame = (timeFrame: number) => {
  if (timeFrame === 0) return moment().startOf('day').toDate();
  else if (timeFrame === 1) return moment().add(-24, 'hours').toDate();
  else if (timeFrame === 2) return moment().add(-48, 'hours').toDate();
  return undefined;
};
