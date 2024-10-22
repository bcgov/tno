import { ISortBy } from 'features/interfaces';
import { ContentTypeName } from 'tno-core';

export interface IContentListFilter {
  pageIndex: number;
  pageSize: number;
  onlyPublished: boolean;
  hasTopic: boolean;
  isHidden: boolean | '';
  ownerId: number | '';
  userId: number | '';
  timeFrame: number | '';
  startDate?: string | null;
  otherSource: string;
  contentTypes: ContentTypeName[];
  mediaTypeIds: number[];
  sourceIds: number[];
  excludeSourceIds: number[];
  contentIds?: number[];
  // Actions
  showOnly?: string;
  commentary: boolean;
  topStory: boolean;
  featuredStory: boolean;
  sort: ISortBy[];
  pendingTranscript?: boolean;
}
