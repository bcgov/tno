import { ContentStatusName } from '@/shared';

import { PublishedStatuses } from '../constants';

export const changeStatus = (status: ContentStatusName) => {
  if (PublishedStatuses.includes(status)) return ContentStatusName.Unpublish;
  return ContentStatusName.Publish;
};
