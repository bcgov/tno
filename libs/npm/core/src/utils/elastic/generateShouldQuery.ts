import { IFilterSettingsModel } from '../../hooks';
import { generateQueryShouldValues } from './generateQueryShouldValues';

/**
 * Generates an Elasticsearch query based on specified 'query'.
 * @param settings Form values that will be used to configure the elasticsearch query.
 * @param query Original query object.
 * @returns Elasticsearch query JSON.
 */
export const generateShouldQuery = (
  fields: string[],
  settings: Partial<IFilterSettingsModel>,
  values: string[],
) => {
  if (!!settings.inHeadline && !!settings.inByline && !!settings.inStory) {
    return {
      bool: {
        should: generateQueryShouldValues(fields, values),
        minimum_should_match: 1,
      },
    };
  }
};
