import { ContentTypeName } from 'hooks';

/**
 * Get the URL path for the specified `contentType`.
 * @param combined Whether the path is the combined page.
 * @param id The content id.
 * @param contentType The content type.
 * @returns The URL path.
 */
export const getContentPath = (
  combined: boolean,
  id: number | string = 0,
  contentType?: ContentTypeName,
) => {
  if (combined) return `/contents/combined/${id}`;

  switch (contentType) {
    case ContentTypeName.Snippet:
      return `/snippets/${id}`;
    case ContentTypeName.PrintContent:
      return `/papers/${id}`;
    case ContentTypeName.Image:
      return `/images/${id}`;
    case ContentTypeName.Story:
      return `/stories/${id}`;
    default:
      return '';
  }
};
