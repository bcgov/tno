import { ContentTypeName } from 'tno-core';

import { ISortBy } from '../../list-view/interfaces';

export interface IMorningReportsFilter {
  pageIndex: number;
  pageSize: number;
  hasTopic: boolean;
  includeHidden: boolean;
  onlyHidden: boolean;
  onlyPublished: boolean;
  contentTypes: ContentTypeName[];
  sourceIds: number[];
  otherSource: string;
  productIds: number[];
  ownerId: number | '';
  userId: number | '';
  timeFrame: number | '';
  // Actions
  onTicker: boolean;
  commentary: boolean;
  topStory: boolean;
  sort: ISortBy[];
}
