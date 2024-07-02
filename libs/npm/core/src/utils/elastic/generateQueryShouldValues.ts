import { IFilterSettingsModel } from '../../hooks';
import { generateMatch } from './generateMatch';
/**
 * Generates an Elasticsearch query based on specified 'query'.
 * @param settings Form values that will be used to configure the elasticsearch query.
 * @param query Original query object.
 * @returns Elasticsearch query JSON.
 */
export const generateQueryShouldValues = (
  settings: Omit<IFilterSettingsModel, 'size' | 'searchUnpublished'>,
): any[] => {
  const matches = generateMatch('byline', settings.contributorNames);
  return matches.flat();
};
