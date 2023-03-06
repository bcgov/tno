import { IContentListFilter } from '../interfaces';
import { defaultPage } from './defaultPage';

export const defaultFilter: IContentListFilter = {
  pageIndex: defaultPage.pageIndex,
  pageSize: defaultPage.pageSize,
  includedInTopic: false,
  includeHidden: false,
  sourceId: 0,
  otherSource: '',
  productIds: [],
  sourceIds: [],
  ownerId: 0,
  userId: '',
  timeFrame: 0,
  onTicker: '',
  commentary: '',
  topStory: '',
  sort: [],
};
