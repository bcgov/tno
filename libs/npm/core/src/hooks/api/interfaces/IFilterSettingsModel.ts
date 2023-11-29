import { ContentStatusName, ContentTypeName } from '../constants';
import { IFilterActionSettingsModel, ISortField } from '.';

export interface IFilterSettingsModel {
  actions?: IFilterActionSettingsModel[];
  boldKeywords?: boolean;
  commentary?: boolean;
  contentIds?: number[];
  contentTypes?: ContentTypeName[];
  contributorIds?: number[];
  dateOffset?: number;
  defaultSearchOperator?: 'and' | 'or';
  edition?: string;
  endDate?: string;
  from?: number;
  hasTopic?: boolean;
  inByline?: boolean;
  inHeadline?: boolean;
  inStory?: boolean;
  isHidden?: boolean;
  mediaTypeIds?: number[];
  names?: string;
  otherSource?: string;
  ownerId?: number;
  page?: string;
  search?: string;
  searchUnpublished: boolean;
  section?: string;
  sentiment?: number[];
  seriesIds?: number[];
  size: number;
  sort?: ISortField[];
  sourceIds?: number[];
  startDate?: string;
  status?: ContentStatusName;
  tags?: string[];
  topStory?: boolean;
  userId?: number;
}
