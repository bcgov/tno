import { IFilterSettingsModel } from '../../hooks';
import { generateQueryString } from './generateQueryString';
import { generateSimpleQueryString } from './generateSimpleQueryString';

export const generateTextQuery = (
  settings: Omit<IFilterSettingsModel, 'size' | 'searchUnpublished'>,
) => {
  const containsWildCards = settings.search?.includes('*') || settings.search?.includes('?');
  const generateQueryFunction = containsWildCards ? generateQueryString : generateSimpleQueryString;
  if (!settings.search && !settings.headline && !settings.summary && !settings.body)
    return undefined;
  const combinedSearch = [settings.search, ...(settings.contributorNames ?? [])].join(' ');
  if (!!settings.inHeadline && !!settings.inByline && !!settings.inStory) {
    // give an arbitrary weight to the headline, so if it's found there
    // it gets a slightly higher score, as opposed to other fields
    return generateQueryFunction(
      ['headline^5', 'byline', 'summary', 'body', 'series.name'],
      combinedSearch,
      settings.defaultSearchOperator,
    );
  }

  let fields: string[] = [];
  if (settings.inByline || settings.inStory || settings.inHeadline || settings.inProgram) {
    if (!!settings.inByline) fields = [...fields, 'byline'];
    if (!!settings.inStory) fields = [...fields, 'summary', 'body'];
    if (!!settings.inHeadline) fields = [...fields, 'headline'];
    if (!!settings.inProgram) fields = [...fields, 'series.name'];
    return fields.length > 0 ? generateQueryFunction(fields, settings.search) : undefined;
  }

  if (settings.headline) return generateQueryFunction(['headline'], settings.headline);
  if (settings.summary) return generateQueryFunction(['summary'], settings.summary);
  if (settings.body) return generateQueryFunction(['body'], settings.body);
};
