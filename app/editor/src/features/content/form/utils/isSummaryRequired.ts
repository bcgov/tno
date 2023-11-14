import { IContentForm } from '../interfaces';

/**
 * Determine if content.summary is a required field.
 * @param values Content form values, or a string value to review.
 * @returns True if the content.summary field is required.
 */
export const isSummaryRequired = (values: IContentForm) => {
  return values.mediaType?.name !== 'News Radio' && values.mediaType?.name !== 'Events';
};
