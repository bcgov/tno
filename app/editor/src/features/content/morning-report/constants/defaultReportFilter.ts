import { defaultPage } from 'features/content/list-view/constants';
import { ContentTypeName, ISourceModel } from 'hooks';

import { IMorningReportFilter } from '../interfaces';

const findIdByCode = (codes: string[], sources?: ISourceModel[]) => {
  return sources?.filter((source) => codes.includes(source.code)).map((option) => option.id);
};

export const defaultReportFilter = (sources?: ISourceModel[]) => {
  return {
    pageIndex: defaultPage.pageIndex,
    pageSize: defaultPage.pageSize,
    contentType: ContentTypeName.PrintContent,
    includedInTopic: false,
    includeHidden: false,
    sourceId: 0,
    otherSource: '',
    productIds: [],
    sourceIds: findIdByCode(['GLOBE', 'POST', 'PROVINCE', 'TC', 'SUN'], sources) ?? [],
    ownerId: 0,
    userId: '',
    timeFrame: 0,
    onTicker: '',
    commentary: '',
    topStory: '',
    sort: [],
  } as IMorningReportFilter;
};
