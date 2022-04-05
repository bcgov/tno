import { IContentListFilter } from '../interfaces';
import { defaultPage } from './defaultPage';

export const defaultFilter: IContentListFilter = {
  pageIndex: defaultPage.pageIndex,
  pageSize: defaultPage.pageSize,
  mediaTypeId: 0,
  contentTypeId: 0,
  ownerId: 0,
  userId: 0,
  timeFrame: 0,
  included: '',
  onTicker: '',
  commentary: '',
  topStory: '',
};
