import moment from 'moment';
import { Moment } from 'moment';

export const generateRangeForDates = (
  field: string,
  gte?: string | Date | Moment | null,
  lte?: string | Date | Moment | null,
) => {
  if (!!gte && !!lte) {
    return {
      range: {
        [field]: {
          gte: moment(gte),
          lte: moment(lte),
          time_zone: 'US/Pacific',
        },
      },
    };
  } else if (!!gte) {
    return {
      range: {
        [field]: {
          gte: moment(gte),
          time_zone: 'US/Pacific',
        },
      },
    };
  } else if (!!lte) {
    return {
      range: {
        [field]: {
          lte: moment(lte),
          time_zone: 'US/Pacific',
        },
      },
    };
  }
};
