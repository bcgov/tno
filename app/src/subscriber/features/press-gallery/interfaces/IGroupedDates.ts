import { IContentSearchResult } from '@/subscriber/features/utils/interfaces';

export interface IGroupedDates {
  [key: string]: IContentSearchResult[];
}
