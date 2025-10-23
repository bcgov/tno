import moment from 'moment';
import React from 'react';
import {
  ContentStatusName,
  generateQuery,
  generateRangeForDates,
  generateSimpleQueryString,
  generateTerm,
  IFilterSettingsModel,
} from 'tno-core';

import { AdvancedSearchKeys } from '../constants';
import { IContentListAdvancedFilter, IContentListFilter } from '../interfaces';
import { useActionFilters } from '../papers/hooks';
import { getSortBy } from '../utils';

/**
 * Provides method to generate an Elasticsearch query.
 * @returns Function to convert a filter to an Elasticsearch query.
 */
export const useElasticsearch = () => {
  const getActionFilters = useActionFilters();

  return React.useCallback(
    (filter: IContentListFilter & Partial<IContentListAdvancedFilter>) => {
      const { fieldType, searchTerm, secondaryFieldType, secondarySearchTerm, ...rest } =
        filter as IContentListFilter & Partial<IContentListAdvancedFilter> & Record<string, any>;

      const settings: IFilterSettingsModel = {
        searchUnpublished: !rest.onlyPublished,
        size: rest.pageSize,
        from: rest.pageIndex * rest.pageSize,
        contentIds: rest.contentIds,
        mediaTypeIds: rest.mediaTypeIds ?? undefined,
        sourceIds: rest.sourceIds ?? undefined,
        contentTypes: rest.contentTypes ?? [],
        ownerId: +rest.ownerId !== 0 ? +rest.ownerId : undefined,
        userId: +rest.userId !== 0 ? +rest.userId : undefined,
        hasTopic: rest.hasTopic,
        isHidden: rest.isHidden !== '' ? rest.isHidden : undefined,
        status: rest.onlyPublished ? ContentStatusName.Published : undefined,
        startDate: rest.startDate ? moment(rest.startDate).toISOString() : undefined,
        endDate: rest.endDate ? moment(rest.endDate).toISOString() : undefined,
        dateOffset:
          rest.startDate || rest.endDate || rest.timeFrame === 3
            ? undefined
            : rest.timeFrame
            ? +rest.timeFrame
            : 0,
        actions: getActionFilters(rest),
        seriesIds: [],
        contributorIds: [],
        tags: [],
        sentiment: [],
        sort: getSortBy(rest.sort),
      };

      const createClause = (type?: AdvancedSearchKeys, term?: string): any => {
        if (!type || !term) return undefined;
        const defaultOperator = 'and';
        switch (type) {
          case AdvancedSearchKeys.Id:
            return generateTerm('id', Number(term));
          case AdvancedSearchKeys.Headline:
            return generateSimpleQueryString(['headline^5'], term, {
              default_operator: defaultOperator,
            });
          case AdvancedSearchKeys.Summary:
            return generateSimpleQueryString(['summary'], term, {
              default_operator: defaultOperator,
            });
          case AdvancedSearchKeys.Story:
            return generateSimpleQueryString(['summary', 'body'], term, {
              default_operator: defaultOperator,
            });
          case AdvancedSearchKeys.Body:
            return generateSimpleQueryString(['body'], term, {
              default_operator: defaultOperator,
            });
          case AdvancedSearchKeys.Byline:
            return generateSimpleQueryString(['byline', 'content.contributor.name'], term, {
              default_operator: defaultOperator,
            });
          case AdvancedSearchKeys.Source:
            return generateTerm('sourceId', Number(term));
          case AdvancedSearchKeys.Series: {
            const sanitized = typeof term === 'string' ? term.trim() : term;
            if (sanitized === '' || sanitized === undefined || sanitized === null) return undefined;
            if (sanitized === '[NONE]') {
              return {
                bool: {
                  should: [
                    {
                      bool: {
                        must_not: [{ exists: { field: 'seriesId' } }],
                      },
                    },
                    {
                      term: { seriesId: 0 },
                    },
                  ],
                  minimum_should_match: 1,
                },
              };
            }
            const value = Number(sanitized);
            if (Number.isNaN(value)) return undefined;
            return generateTerm('seriesId', value);
          }
          case AdvancedSearchKeys.Section:
            return generateTerm('section', term);
          case AdvancedSearchKeys.Page:
            return generateTerm('page', term);
          case AdvancedSearchKeys.Status:
            return generateTerm('status', term);
          case AdvancedSearchKeys.Edition:
            return generateTerm('edition', term);
          case AdvancedSearchKeys.ContentType:
            return generateTerm('contentType', term);
          case AdvancedSearchKeys.PublishedOn:
            return moment(term).isValid()
              ? generateRangeForDates(
                  'publishedOn',
                  moment(term).startOf('day'),
                  moment(term).endOf('day'),
                )
              : undefined;
          case AdvancedSearchKeys.CreatedOn:
            return moment(term).isValid()
              ? generateRangeForDates(
                  'createdOn',
                  moment(term).startOf('day'),
                  moment(term).endOf('day'),
                )
              : undefined;
          case AdvancedSearchKeys.UpdatedOn:
            return moment(term).isValid()
              ? generateRangeForDates(
                  'updatedOn',
                  moment(term).startOf('day'),
                  moment(term).endOf('day'),
                )
              : undefined;
          default:
            return undefined;
        }
      };

      const appendClause = (query: any, clause?: any) => {
        if (!clause) return query;
        const existingMust = Array.isArray(query?.query?.bool?.must)
          ? [...query.query.bool.must]
          : query?.query?.bool?.must
          ? [query.query.bool.must]
          : [];
        existingMust.push(clause);
        return {
          ...query,
          query: {
            ...(query.query ?? {}),
            bool: {
              ...(query.query?.bool ?? {}),
              must: existingMust,
            },
          },
        };
      };

      let elastic = generateQuery(settings);
      elastic = appendClause(elastic, createClause(fieldType, searchTerm));
      elastic = appendClause(elastic, createClause(secondaryFieldType, secondarySearchTerm));

      return elastic;
    },
    [getActionFilters],
  );
};
