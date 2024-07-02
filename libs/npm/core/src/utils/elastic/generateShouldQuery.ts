import { IFilterSettingsModel } from '../../hooks';
import { generateQueryShouldValues } from './generateQueryShouldValues';

/**
 * Generates an Elasticsearch query based on specified 'query'.
 * @param settings Form values that will be used to configure the elasticsearch query.
 * @param query Original query object.
 * @returns Elasticsearch query JSON.
 */
export const generateShouldQuery = (settings: Partial<IFilterSettingsModel>) => {
  return {
    bool: {
      should: generateQueryShouldValues(settings),
      minimum_should_match: 1,
    },
  };
};
