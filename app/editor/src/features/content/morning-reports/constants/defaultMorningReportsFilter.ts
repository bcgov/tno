import { defaultPage } from 'features/content/list-view/constants';
import { ContentTypeName, ISourceModel } from 'tno-core';

import { IMorningReportsFilter } from '../interfaces';
import { defaultSources } from '.';

/**
 * Creates a default morning report filter.
 * @param sources An array of sources.
 * @returns Morning report filter.
 */
export const defaultMorningReportsFilter = (
  sources: ISourceModel[] = [],
): IMorningReportsFilter => {
  return {
    pageIndex: defaultPage.pageIndex,
    pageSize: defaultPage.pageSize,
    contentTypes: [ContentTypeName.PrintContent, ContentTypeName.Image],
    hasTopic: false,
    includeHidden: false,
    onlyHidden: false,
    onlyPublished: false,
    otherSource: '',
    productIds: [],
    sourceIds: defaultSources(sources).map((s) => s.id),
    ownerId: 0,
    userId: '',
    timeFrame: 0,
    onTicker: false,
    commentary: false,
    topStory: false,
    sort: [],
  };
};
