import { IFilterSettingsModel } from '../../hooks';
import { generatePublishedOnQuery } from './generatePublishedOnQuery';
import { generateQueryForActions } from './generateQueryForActions';
import { generateQueryForExistCheck } from './generateQueryForExistCheck';
import { generateRangeForArrayField } from './generateRangeForArrayField';
import { generateTerm } from './generateTerm';
import { generateTerms } from './generateTerms';
import { generateTermsForArrayField } from './generateTermsForArrayField';
import { generateTextQuery } from './generateTextQuery';

/**
 * Generates an Elasticsearch query based on specified 'query'.
 * @param settings Form values that will be used to configure the elasticsearch query.
 * @param query Original query object.
 * @returns Elasticsearch query JSON.
 */
export const generateQuery = (settings: IFilterSettingsModel, query: any = {}) => {
  var elastic = { ...query };
  if (settings.size) elastic = { ...elastic, size: settings.size };
  if (settings.from) elastic = { ...elastic, from: settings.from };

  const actionFilters = generateQueryForActions(settings.actions ?? []);
  elastic = {
    ...elastic,
    query: {
      bool: {
        must: [
          generatePublishedOnQuery(settings),
          generateTerms('sourceId', settings.sourceIds),
          generateTerms('mediaTypeId', settings.mediaTypeIds),
          generateTerms('seriesId', settings.seriesIds),
          generateTerms('contributorId', settings.contributorIds),
          generateTerms('contentType', settings.contentTypes),
          generateTerms('id', settings.contentIds),
          generateTermsForArrayField('tags.code', settings.tags),
          generateRangeForArrayField('tonePools.value', settings.sentiment),
          actionFilters.length > 1 ? { bool: { should: actionFilters } } : undefined,
          actionFilters.length === 1 ? actionFilters[0] : undefined,
          generateTextQuery(settings),
          settings.edition ? generateTerm('edition', settings.edition) : undefined,
          settings.section ? generateTerm('section', settings.section) : undefined,
          settings.page ? generateTerm('page', settings.page) : undefined,
          settings.status ? generateTerm('status', settings.status) : undefined,
          settings.userId ? generateTerm('ownerId', +settings.userId) : undefined,
          settings.hasTopic ? generateQueryForExistCheck('topics') : undefined,
          settings.isHidden !== undefined ? generateTerm('isHidden', settings.isHidden) : undefined,
        ].filter((v) => v !== undefined),
      },
    },
  };

  elastic = { ...elastic, sort: settings.sort ? settings.sort : [{ publishedOn: 'desc' }] };

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
