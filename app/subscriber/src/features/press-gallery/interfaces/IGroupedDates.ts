import { IContentSearchResult } from 'features/utils/interfaces';

export interface IGroupedDates {
  [key: string]: IContentSearchResult[];
}
