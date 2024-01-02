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
export const generateQueryValues = (
  settings: Omit<IFilterSettingsModel, 'size' | 'searchUnpublished'>,
): any[] => {
  const actionFilters = generateQueryForActions(settings.actions ?? []);
  const values = [
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
    settings.id ? generateTerm('id', settings.id) : undefined,
    settings.isHidden !== undefined ? generateTerm('isHidden', settings.isHidden) : undefined,
  ].filter((v) => v !== undefined);
  return values;
};
