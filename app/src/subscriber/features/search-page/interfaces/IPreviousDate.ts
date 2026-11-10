import { type Dayjs } from '@/shared/utils/date';

export interface IPreviousDate {
  startDate: Dayjs;
  endDate: Dayjs;
  hits: number;
}
