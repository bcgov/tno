import { IContentListFilter } from '../interfaces';
import { defaultPage } from './defaultPage';

export const defaultFilter: IContentListFilter = {
  pageIndex: defaultPage.pageIndex,
  pageSize: defaultPage.pageSize,
  hasTopic: false,
  includeHidden: false,
  onlyHidden: false,
  onlyPublished: false,
  otherSource: '',
  contentTypes: [],
  productIds: [],
  sourceIds: [],
  ownerId: 0,
  userId: '',
  timeFrame: 0,
  onTicker: false,
  commentary: false,
  topStory: false,
  sort: [],
};
