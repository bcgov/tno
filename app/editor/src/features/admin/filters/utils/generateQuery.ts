import moment from 'moment';
import { IFilterSettingsModel } from 'tno-core';

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
          ...generateDateSearch('publishedOn', settings),
          ...generateTerms('sourceId', settings.sourceIds),
          ...generateTerms('productId', settings.productIds),
          ...generateTerms('seriesId', settings.seriesIds),
          ...generateTerms('contributorId', settings.contributorIds),
          ...generateTextSearch(settings),
        ],
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

const generateTerms = (key: string, values?: any[]) => {
  return values && values.length > 0
    ? [
        {
          terms: { [key]: values },
        },
      ]
    : [];
};

const generateTextSearch = (settings: IFilterSettingsModel) => {
  if (!settings.search) return [];
  if (settings.searchIn === 'all')
    return [
      {
        multi_match: {
          query: settings.search,
          fields: ['headline', 'byline', 'summary', 'body'],
        },
      },
    ];
  if (settings.searchIn === 'story')
    return [
      {
        multi_match: {
          query: settings.search,
          fields: ['summary', 'body'],
        },
      },
    ];
  if (settings.searchIn === 'headline')
    return [
      {
        match: {
          headline: settings.search,
        },
      },
    ];
  if (settings.searchIn === 'byline')
    return [
      {
        match: {
          byline: settings.search,
        },
      },
    ];

  return [];
};

const generateDateSearch = (key: string, settings: IFilterSettingsModel) => {
  if (settings.dateOffset !== undefined)
    return [
      {
        range: {
          [key]: {
            gte: `now${
              settings.dateOffset > 0
                ? `+${settings.dateOffset}d/d`
                : settings.dateOffset < 0
                ? `${settings.dateOffset}d/d`
                : '/d'
            }`,
            time_zone: 'US/Pacific',
          },
        },
      },
    ];
  if (settings.startDate && settings.endDate)
    return [
      {
        range: {
          [key]: {
            gte: moment(settings.startDate).format('yyyy-MM-DD'),
            lte: moment(settings.endDate).format('yyyy-MM-DD'),
            time_zone: 'US/Pacific',
          },
        },
      },
    ];
  if (settings.startDate)
    return [
      {
        range: {
          [key]: {
            gte: moment(settings.startDate).format('yyyy-MM-DD'),
            lte: moment(settings.startDate).format('yyyy-MM-DD'),
            time_zone: 'US/Pacific',
          },
        },
      },
    ];
  if (settings.endDate)
    return [
      {
        range: {
          [key]: {
            gte: moment(settings.endDate).format('yyyy-MM-DD'),
            lte: moment(settings.endDate).format('yyyy-MM-DD'),
            time_zone: 'US/Pacific',
          },
        },
      },
    ];
  return [];
};
