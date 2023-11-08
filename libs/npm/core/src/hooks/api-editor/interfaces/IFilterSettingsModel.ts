import { ContentStatusName, ContentTypeName } from '../constants';
import { IFilterActionSettingsModel, ISortField } from '.';

export interface IFilterSettingsModel {
  searchUnpublished: boolean;
  size: number;
  from?: number;
  status?: ContentStatusName;
  search?: string;
  defaultSearchOperator?: 'and' | 'or';
  inHeadline?: boolean;
  inByline?: boolean;
  inStory?: boolean;
  startDate?: string;
  endDate?: string;
  dateOffset?: number;
  edition?: string;
  section?: string;
  page?: string;
  hasTopic?: boolean;
  topStory?: boolean;
  isHidden?: boolean;
  otherSource?: string;
  ownerId?: number;
  userId?: number;
  contentIds?: number[];
  sourceIds?: number[];
  mediaTypeIds?: number[];
  seriesIds?: number[];
  contributorIds?: number[];
  actions?: IFilterActionSettingsModel[];
  contentTypes?: ContentTypeName[];
  tags?: string[];
  sentiment?: number[];
  sort?: ISortField[];
}
