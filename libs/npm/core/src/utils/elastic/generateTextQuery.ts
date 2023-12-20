import { IFilterSettingsModel } from '../../hooks';
import { generateSimpleQueryString } from './generateSimpleQueryString';

export const generateTextQuery = (
  settings: Omit<IFilterSettingsModel, 'size' | 'searchUnpublished'>,
) => {
  if (!settings.search) return undefined;
  if (!!settings.inHeadline && !!settings.inByline && !!settings.inStory) {
    // give an arbitrary weight to the headline, so if it's found there
    // it gets a slightly higher score, as opposed to other fields
    return generateSimpleQueryString(
      ['headline^5', 'byline', 'summary', 'body'],
      settings.search,
      settings.defaultSearchOperator,
    );
  }

  let fields: string[] = [];
  if (!!settings.inByline) fields = [...fields, 'byline'];
  if (!!settings.inStory) fields = [...fields, 'summary', 'body'];
  if (!!settings.inHeadline) fields = [...fields, 'headline'];
  return fields.length > 0 ? generateSimpleQueryString(fields, settings.search) : undefined;
};
