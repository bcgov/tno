import { IFilterSettingsModel } from '../../hooks';
import { generateQueryString } from './generateQueryString';
import { generateSimpleQueryString } from './generateSimpleQueryString';

export const generateTextQuery = (
  settings: Omit<IFilterSettingsModel, 'size' | 'searchUnpublished'>,
) => {
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

  if (
    settings.search &&
    (settings.inByline || settings.inStory || settings.inHeadline || settings.inProgram || settings.inTags)
  ) {
    let fields: string[] = [];
    if (settings.inByline) fields = [...fields, 'byline', 'content.contributor.name'];
    if (settings.inStory) fields = [...fields, 'summary', 'body'];
    if (settings.inHeadline) fields = [...fields, 'headline^5'];
    if (settings.inProgram) fields = [...fields, 'series.name'];
    
    // check if inTags is true
    if (settings.inTags) {
      // create combined query: regular fields + tag fields (using bool.should to implement OR relationship)
      return {
        bool: {
          should: [
            // regular fields query (if fields are enabled)
            fields.length > 0
              ? generateQueryFunction(fields, settings.search, {
                  default_operator: settings.defaultOperator,
                })
              : undefined,
            
            // tag nested query
            {
              nested: {
                path: 'tags', // nested path is tags
                query: {
                  match: {
                    'tags.code': { // search tag code field
                      query: settings.search,
                      operator: 'or',
                      case_sensitive: true
                    }
                  }
                }
              }
            }
          ].filter(Boolean), // remove undefined values
          minimum_should_match: 1 // at least one condition must match
        }
      };
    } else {
      // use regular search logic ( original logic)
      return fields.length > 0
        ? generateQueryFunction(fields, settings.search, {
            default_operator: settings.defaultOperator,
          })
        : undefined;
    }
  } else if (settings.headline) {
    return generateQueryFunction(['headline'], settings.headline, {
      default_operator: settings.defaultOperator,
    });
  } else if (settings.summary) {
    return generateQueryFunction(['summary'], settings.summary, {
      default_operator: settings.defaultOperator,
    });
  } else if (settings.body) {
    return generateQueryFunction(['body'], settings.body, {
      default_operator: settings.defaultOperator,
    });
  }

  return undefined;
};
