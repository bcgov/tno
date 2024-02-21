import { IContentSearchResult } from 'features/utils/interfaces';

export interface IGroupedDates {
  date: string;
  content: IContentSearchResult[];
}
