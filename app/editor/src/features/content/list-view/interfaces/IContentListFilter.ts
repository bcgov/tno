export interface IContentListFilter {
  pageIndex: number;
  pageSize: number;
  mediaTypeId: number;
  contentTypeId: number;
  ownerId: number | '';
  userId: number | '';
  timeFrame: number | '';
  // Actions
  included: string;
  onTicker: string;
  commentary: string;
  topStory: string;
}
