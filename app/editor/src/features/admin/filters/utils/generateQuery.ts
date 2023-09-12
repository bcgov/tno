import { IFilterSettingsModel } from 'tno-core';

import {
  generateQueryForActions,
  generateRangeForArrayField,
  generateRangeForDateOffset,
  generateRangeForDates,
  generateSimpleQueryString,
  generateTerm,
  generateTerms,
  generateTermsForArrayField,
} from '.';

/**
 * Generates an Elasticsearch query based on specified 'query'.
 * @param settings Form values that will be used to configure the elasticsearch query.
 * @param query Original query object.
 * @returns Elasticsearch query JSON.
 */
export const generateQuery = (settings: IFilterSettingsModel, query: any = {}) => {
  var elastic = { ...query };
  if (settings.size) elastic = { ...elastic, size: settings.size };

  elastic = {
    ...elastic,
    query: {
      bool: {
        must: [
          generatePublishedOnQuery(settings),
          generateTerms('sourceId', settings.sourceIds),
          generateTerms('productId', settings.productIds),
          generateTerms('seriesId', settings.seriesIds),
          generateTerms('contributorId', settings.contributorIds),
          generateTerms('contentType', settings.contentTypes),
          generateTermsForArrayField('tags.code', settings.tags),
          generateRangeForArrayField('tonePools.value', settings.sentiment),
          ...generateQueryForActions(settings.actions ?? []),
          generateTextQuery(settings),
          generateTerm('edition', settings.edition),
          generateTerm('section', settings.section),
          generateTerm('page', settings.page),
        ].filter((v) => v !== undefined),
      },
    },
  };

  // Remove any empty paths.
  if (elastic.query) {
    if (JSON.stringify(elastic.query.term) === '{}') elastic.query.term = undefined;
    if (JSON.stringify(elastic.query.multi_match) === '{}') elastic.query.multi_match = undefined;
    if (JSON.stringify(elastic.query.terms) === '{}') elastic.query.terms = undefined;
    if (elastic.query.bool) {
      if (JSON.stringify(elastic.query.bool.must) === '[]') elastic.query.bool.must = undefined;
      if (JSON.stringify(elastic.query.bool.should) === '[]') elastic.query.bool.should = undefined;
      if (JSON.stringify(elastic.query.bool) === '{}') elastic.query.bool = undefined;
    }
    if (JSON.stringify(elastic.query) === '{}') elastic.query = undefined;
  }

  return elastic;
};

const generateTextQuery = (settings: IFilterSettingsModel) => {
  if (!settings.search) return undefined;
  switch (settings.searchIn) {
    case 'all':
      // give an arbitrary weight to the headline, so if it's found there
      // it gets a slightly higher score, as opposed to other fields
      return generateSimpleQueryString(
        ['headline^5', 'byline', 'summary', 'body'],
        settings.search,
      );
    case 'story':
      return generateSimpleQueryString(['summary', 'body'], settings.search);
    case 'headline':
      return generateSimpleQueryString(['headline'], settings.search);
    case 'byline':
      return generateSimpleQueryString(['byline'], settings.search);
    default:
      return undefined;
  }
};

const generatePublishedOnQuery = (settings: IFilterSettingsModel) => {
  if (settings.dateOffset !== undefined)
    return generateRangeForDateOffset('publishedOn', settings.dateOffset);
  if (settings.startDate && settings.endDate)
    return generateRangeForDates('publishedOn', [settings.startDate, settings.endDate]);
  if (settings.startDate) return generateRangeForDates('publishedOn', [settings.startDate]);
  if (settings.endDate) return generateRangeForDates('publishedOn', [settings.endDate]);
};
