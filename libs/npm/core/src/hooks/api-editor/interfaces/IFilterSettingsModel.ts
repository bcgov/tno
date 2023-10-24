import { ContentTypeName } from '../constants';
import { IFilterActionSettingsModel } from '.';

export interface IFilterSettingsModel extends Record<string, any> {
  searchUnpublished: boolean;
  size: number;
  from: number;
  search?: string;
  defaultSearchOperator?: 'and' | 'or';
  inHeadline: boolean;
  inByline: boolean;
  inStory: boolean;
  startDate?: string;
  endDate?: string;
  dateOffset?: number;
  edition?: string;
  section?: string;
  page?: string;
  sourceIds: number[];
  productIds: number[];
  seriesIds: number[];
  contributorIds: number[];
  actions: IFilterActionSettingsModel[];
  contentTypes: ContentTypeName[];
  tags: string[];
  sentiment: number[];
  hasTopic?: boolean;
  status?: string;
  userId?: number;
}
