import { ContentTypeName } from 'tno-core';

/**
 * Get the URL path for the specified `contentType`.
 * @param combined Whether the path is the combined page.
 * @param id The content id.
 * @param contentType The content type.
 * @param combinedPath Root path of combined view (default: /contents).
 * @returns The URL path.
 */
export const getContentPath = (
  combined: boolean,
  id: number | string = 0,
  contentType?: ContentTypeName,
  combinedPath: string = '/contents/combined',
) => {
  if (combined) return `${combinedPath}/${id}${window.location.search}`;

  switch (contentType) {
    case ContentTypeName.Snippet:
      return `/snippets/${id}${window.location.search}`;
    case ContentTypeName.PrintContent:
      return `/papers/${id}${window.location.search}`;
    case ContentTypeName.Image:
      return `/images/${id}${window.location.search}`;
    case ContentTypeName.Story:
      return `/stories/${id}${window.location.search}`;
    default:
      return '';
  }
};
