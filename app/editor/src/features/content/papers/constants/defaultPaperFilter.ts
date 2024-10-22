import { IContentListFilter } from 'features/content/interfaces';
import { defaultPage } from 'features/content/list-view/constants';
import moment from 'moment';
import { ContentTypeName, ISourceModel } from 'tno-core';

import { defaultSort } from './defaultSort';

/**
 * Creates a default paper filter.
 * @param sources An array of sources.
 * @returns Paper filter.
 */
export const defaultPaperFilter = (sources: ISourceModel[] = []): IContentListFilter => {
  // Return stories that are also from yesterday if published after 10PM.
  const yesterdayAt10PM = moment()
    .subtract(1, 'days')
    .set({ hour: 22, minute: 0, second: 0, millisecond: 0 });

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
    timeFrame: '',
    startDate: yesterdayAt10PM.format(),
    commentary: false,
    topStory: false,
    featuredStory: false,
    sort: defaultSort,
  };
};
