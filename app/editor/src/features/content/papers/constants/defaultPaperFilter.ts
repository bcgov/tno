import { IContentListFilter } from 'features/content/interfaces';
import { defaultPage } from 'features/content/list-view/constants';
import { ContentTypeName, ISourceModel } from 'tno-core';

import { defaultSort } from './defaultSort';

/**
 * Creates a default paper filter.
 * @param sources An array of sources.
 * @returns Paper filter.
 */
export const defaultPaperFilter = (sources: ISourceModel[] = []): IContentListFilter => {
  return {
    pageIndex: defaultPage.pageIndex,
    pageSize: defaultPage.pageSize,
    contentTypes: [ContentTypeName.PrintContent],
    hasTopic: false,
    isHidden: false,
    onlyPublished: false,
    otherSource: '',
    mediaTypeIds: [],
    excludeSourceIds: [],
    sourceIds: sources.map((s) => s.id),
    ownerId: 0,
    userId: '',
    timeFrame: 0,
    commentary: false,
    topStory: false,
    featuredStory: false,
    sort: defaultSort,
  };
};
