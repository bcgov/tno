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
    (settings.inByline || settings.inStory || settings.inHeadline || settings.inProgram)
  ) {
    let fields: string[] = [];
    if (settings.inByline) fields = [...fields, 'byline', 'content.contributor.name'];
    if (settings.inStory) fields = [...fields, 'summary', 'body'];
    if (settings.inHeadline) fields = [...fields, 'headline^5'];
    if (settings.inProgram) fields = [...fields, 'series.name'];
    return fields.length > 0
      ? generateQueryFunction(fields, settings.search, {
          default_operator: settings.defaultOperator,
        })
      : undefined;
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
