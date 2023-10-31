import moment from 'moment';
import React from 'react';
import { useLookup } from 'store/hooks';
import { ContentStatusName, generateQuery, IFilterSettingsModel } from 'tno-core';

import { AdvancedSearchKeys } from '../constants';
import { IContentListAdvancedFilter, IContentListFilter } from '../interfaces';
import { getActionFilters, getSortBy } from '../utils';

/**
 * Provides method to generate an Elasticsearch query.
 * @returns Function to convert a filter to an Elasticsearch query.
 */
export const useElasticsearch = () => {
  const [{ actions }] = useLookup();

  return React.useCallback(
    (filter: IContentListFilter & Partial<IContentListAdvancedFilter>) => {
      const settings: IFilterSettingsModel = {
        searchUnpublished: !filter.onlyPublished,
        size: filter.pageSize,
        from: filter.pageIndex * filter.pageSize,
        productIds: filter.productIds ?? undefined,
        sourceIds: filter.sourceIds ?? undefined,
        contentTypes: filter.contentTypes ?? [],
        ownerId: +filter.ownerId !== 0 ? +filter.ownerId : undefined,
        userId: +filter.userId !== 0 ? +filter.userId : undefined,
        hasTopic: filter.hasTopic,
        isHidden: filter.isHidden !== '' ? filter.isHidden : undefined,
        status: filter.onlyPublished ? ContentStatusName.Published : undefined,
        startDate: filter.startDate ? moment(filter.startDate).toISOString() : undefined,
        endDate: filter.endDate ? moment(filter.endDate).toISOString() : undefined,
        dateOffset:
          filter.startDate || filter.endDate ? undefined : filter.timeFrame ? +filter.timeFrame : 0,
        actions: getActionFilters(filter, actions),
        seriesIds: [],
        contributorIds: [],
        tags: [],
        sentiment: [],
        sort: getSortBy(filter.sort),
      };
      if (filter.fieldType && filter.searchTerm) {
        if (filter.fieldType === AdvancedSearchKeys.Page) settings.page = filter.searchTerm;
        else if (filter.fieldType === AdvancedSearchKeys.Source)
          settings.sourceIds = [+filter.searchTerm];
        else if (
          [
            AdvancedSearchKeys.Story,
            AdvancedSearchKeys.Headline,
            AdvancedSearchKeys.Byline,
          ].includes(filter.fieldType)
        ) {
          settings.search = filter.searchTerm;
          settings.inHeadline = filter.fieldType === AdvancedSearchKeys.Headline;
          settings.inByline = filter.fieldType === AdvancedSearchKeys.Byline;
          settings.inStory = filter.fieldType === AdvancedSearchKeys.Story;
        } else (settings as any)[filter.fieldType] = filter.searchTerm; // TODO: Remove 'any' and resolve remaining filter types.
      }
      return generateQuery(settings);
    },
    [actions],
  );
};
