import { IFilterSettingsModel } from '../../hooks';
import { generateQueryString } from './generateQueryString';
import { generateSimpleQueryString } from './generateSimpleQueryString';

export const generateTextQuery = (
  settings: Omit<IFilterSettingsModel, 'size' | 'searchUnpublished'>,
) => {
  const containsWildCards = settings.search?.includes('*') || settings.search?.includes('?');
  const generateQueryFunction = containsWildCards ? generateQueryString : generateSimpleQueryString;
  if (
    !settings.search &&
    !settings.headline &&
    !settings.summary &&
    !settings.body &&
    !settings.inProgram
  )
    return undefined;

  let fields: string[] = [];
  if (settings.inByline || settings.inStory || settings.inHeadline || settings.inProgram) {
    if (!!settings.inByline) fields = [...fields, 'byline', 'content.contributor.name'];
    if (!!settings.inStory) fields = [...fields, 'summary', 'body'];
    if (!!settings.inHeadline) fields = [...fields, 'headline^5'];
    if (!!settings.inProgram) fields = [...fields, 'series.name'];
    return fields.length > 0
      ? generateQueryFunction(fields, settings.search, settings.defaultSearchOperator)
      : undefined;
  }

  if (settings.headline)
    return generateQueryFunction(['headline'], settings.headline, settings.defaultSearchOperator);
  if (settings.summary)
    return generateQueryFunction(['summary'], settings.summary, settings.defaultSearchOperator);
  if (settings.body)
    return generateQueryFunction(['body'], settings.body, settings.defaultSearchOperator);
};
