import { defaultPage } from 'features/content/list-view/constants';
import { ContentTypeName } from 'hooks';

import { IMorningReportFilter } from '../interfaces';

export const defaultReportFilter: IMorningReportFilter = {
  pageIndex: defaultPage.pageIndex,
  pageSize: defaultPage.pageSize,
  contentType: ContentTypeName.PrintContent,
  includedInTopic: false,
  includeHidden: false,
  sourceId: 0,
  otherSource: '',
  productIds: [],
  sourceIds: [91, 248, 249, 246, 247],
  ownerId: 0,
  userId: '',
  timeFrame: 0,
  onTicker: '',
  commentary: '',
  topStory: '',
  sort: [],
};
