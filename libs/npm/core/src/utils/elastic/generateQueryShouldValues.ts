import { IFilterSettingsModel } from '../../hooks';
import { generateMatchPhrase } from './generateMatchPhrase';
/**
 * Generates an Elasticsearch query based on specified 'query'.
 * @param settings Form values that will be used to configure the elasticsearch query.
 * @param query Original query object.
 * @returns Elasticsearch query JSON.
 */
export const generateQueryShouldValues = (fields: string[], values: string[]): any[] => {
  const matches = generateMatchPhrase(fields, values);
  return matches.flat();
};
