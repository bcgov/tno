import { IOptionItem } from 'components/form';

export interface IContentListFilter {
  pageIndex: number;
  pageSize: number;
  mediaTypeId: number;
  contentTypeId: number;
  ownerId: number | '';
  userId: number | '';
  timeFrame: IOptionItem;
  // Actions
  included: string;
  onTicker: string;
  commentary: string;
  topStory: string;
}
