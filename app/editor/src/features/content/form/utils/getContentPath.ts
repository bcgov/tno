import { ContentTypeName } from 'tno-core';

/**
 * Get the URL path for the specified `contentType`.
 * @param id The content id.
 * @param contentType The content type.
 * @returns The URL path.
 */
export const getContentPath = (id: number | string = 0, contentType?: ContentTypeName) => {
  switch (contentType) {
    case ContentTypeName.AudioVideo:
      return `/snippets/${id}${window.location.search}`;
    case ContentTypeName.PrintContent:
      return `/papers/${id}${window.location.search}`;
    case ContentTypeName.Image:
      return `/images/${id}${window.location.search}`;
    case ContentTypeName.Internet:
      return `/stories/${id}${window.location.search}`;
    default:
      return '';
  }
};
