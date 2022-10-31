import { ContentTypeName } from 'hooks';

/**
 * Determine if the content should use the image form.
 * @param contentType The content type.
 * @returns True if the content should use the image form.
 */
export const isImageForm = (contentType: ContentTypeName) => {
  return contentType === ContentTypeName.Image;
};
