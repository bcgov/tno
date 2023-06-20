import { defaultPage } from 'features/content/list-view/constants';
import { ContentTypeName, ISourceModel } from 'tno-core';

import { IPaperFilter } from '../interfaces';
import { defaultSources } from '.';

/**
 * Creates a default paper filter.
 * @param sources An array of sources.
 * @returns Paper filter.
 */
export const defaultPaperFilter = (sources: ISourceModel[] = []): IPaperFilter => {
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
    excludeSourceIds: [],
    sourceIds: defaultSources(sources).map((s) => s.id),
    ownerId: 0,
    userId: '',
    timeFrame: 0,
    onTicker: false,
    commentary: false,
    topStory: false,
    homepage: false,
    sort: [],
  };
};
