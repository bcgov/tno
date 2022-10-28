import { ContentTypeName } from 'hooks';

/**
 * Determine if the content should use the snippet form.
 * @param contentType The content type.
 * @returns True if the content should use the snippet form.
 */
export const isSnippetForm = (contentType: ContentTypeName) => {
  switch (contentType) {
    case ContentTypeName.PrintContent:
      return false;
    case ContentTypeName.Image:
    case ContentTypeName.Story:
    case ContentTypeName.Snippet:
    default:
      return true;
  }
};

export const isImageForm = (contentType: ContentTypeName) => {
  return contentType === ContentTypeName.Image;
};
