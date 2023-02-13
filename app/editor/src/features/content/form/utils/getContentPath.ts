import { ContentTypeName } from 'hooks';

/**
 * Get the URL path for the specified 'contentType'.
 * @param contentType The content type.
 * @returns The URL path.
 */
export const getContentPath = (contentType?: ContentTypeName) => {
  switch (contentType) {
    case ContentTypeName.Snippet:
      return '/snippets/';
    case ContentTypeName.PrintContent:
      return '/papers/';
    case ContentTypeName.Image:
      return '/images/';
    case ContentTypeName.Story:
      return '/stories/';
    default:
      return '';
  }
};
