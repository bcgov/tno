import { IFilterSettingsModel } from '../../hooks';
import { generateQueryString } from './generateQueryString';
import { generateSimpleQueryString } from './generateSimpleQueryString';
import { combineFieldAndTagQueries, generateTagsQuery } from './generateTagsQuery';

/**
 * Generate Elasticsearch text query
 *
 * This function generates Elasticsearch query objects based on user search settings.
 * Main features include:
 * 1. Regular text field search (headline, body, summary, etc.)
 * 2. When inStory is true, automatically search related tags
 *
 * @param settings Search settings parameters
 * @returns Elasticsearch query object or undefined
 */
export const generateTextQuery = (
  settings: Omit<IFilterSettingsModel, 'size' | 'searchUnpublished'>,
) => {
  // If no search criteria are specified, return undefined
  if (
    !settings.search &&
    !settings.headline &&
    !settings.summary &&
    !settings.body &&
    !settings.inProgram
  )
    return undefined;

  const generateQueryFunction =
    settings.queryType === 'query-string' ? generateQueryString : generateSimpleQueryString;

  // Handle main search logic
  if (
    settings.search &&
    (settings.inByline || settings.inStory || settings.inHeadline || settings.inProgram)
  ) {
    // Determine the fields to search based on the search settings
    let fields: string[] = [];
    if (settings.inByline) fields = [...fields, 'byline', 'content.contributor.name'];
    if (settings.inStory) fields = [...fields, 'summary', 'body'];
    if (settings.inHeadline) fields = [...fields, 'headline^5'];
    if (settings.inProgram) fields = [...fields, 'series.name'];

    // Create regular field query
    const fieldsQuery =
      fields.length > 0
        ? generateQueryFunction(fields, settings.search, {
            default_operator: settings.defaultOperator,
          })
        : undefined;

    // If inStory is true, also search for tags
    if (settings.inStory) {
      try {
        const tagsQuery = generateTagsQuery(settings.search);
        return combineFieldAndTagQueries(fieldsQuery, tagsQuery);
      } catch (error) {
        console.error('Error generating tags query.');
        return fieldsQuery;
      }
    }

    // Regular search logic (excluding tag search)
    return fieldsQuery;
  } else if (settings.headline)
    return generateQueryFunction(['headline'], settings.headline, {
      default_operator: settings.defaultOperator,
    });
  else if (settings.summary)
    return generateQueryFunction(['summary'], settings.summary, {
      default_operator: settings.defaultOperator,
    });
  else if (settings.body)
    return generateQueryFunction(['body'], settings.body, {
      default_operator: settings.defaultOperator,
    });

  return undefined;
};
