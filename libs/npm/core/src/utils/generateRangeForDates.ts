import moment from 'moment';
import { Moment } from 'moment';

export const generateRangeForDates = (
  field: string,
  values: string | Date | Moment | string[] | Date[] | Moment[],
) => {
  if (values === undefined || values === null) return undefined;
  if (Array.isArray(values)) {
    const value1 = values[0];
    const values2 = values.length > 1 ? values[1] : values[0];
    return {
      range: {
        [field]: {
          gte: moment(value1).format('yyyy-MM-DD'),
          lte: moment(values2).format('yyyy-MM-DD'),
          time_zone: 'US/Pacific',
          format: 'yyyy-MM-DD',
        },
      },
    };
  } else {
    return {
      range: {
        [field]: {
          gte: moment(values).format('yyyy-MM-DD'),
          lte: moment(values).format('yyyy-MM-DD'),
          time_zone: 'US/Pacific',
          format: 'yyyy-MM-DD',
        },
      },
    };
  }
};
