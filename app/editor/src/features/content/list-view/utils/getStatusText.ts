import { ContentStatusName } from 'tno-core';

/**
 * Provides a different status text for some values.
 * @param status The current status.
 * @returns A status text.
 */
export const getStatusText = (status: ContentStatusName) => {
  switch (status) {
    case ContentStatusName.Unpublish:
      return 'Unpublishing';
    case ContentStatusName.Publish:
      return 'Indexing';
    default:
      return status;
  }
};
