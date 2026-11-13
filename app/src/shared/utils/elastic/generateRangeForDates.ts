import { dayjs, type Dayjs } from '@/shared/utils/date';

export const generateRangeForDates = (
  field: string,
  gte?: string | Date | Dayjs | null,
  lte?: string | Date | Dayjs | null,
) => {
  if (!!gte && !!lte) {
    return {
      range: {
        [field]: {
          gte: dayjs(gte),
          lte: dayjs(lte),
          time_zone: 'America/Vancouver',
        },
      },
    };
  } else if (!!gte) {
    return {
      range: {
        [field]: {
          gte: dayjs(gte),
          time_zone: 'America/Vancouver',
        },
      },
    };
  } else if (!!lte) {
    return {
      range: {
        [field]: {
          lte: dayjs(lte),
          time_zone: 'America/Vancouver',
        },
      },
    };
  }
};
