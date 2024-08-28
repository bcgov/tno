import { INotificationInstanceModel } from 'tno-core';

import { formatDate } from './formatDate';

export const getLastSent = (instance: INotificationInstanceModel) => {
  const sentOn = instance?.sentOn;
  if (!sentOn) return 'Never';
  return formatDate(sentOn, true);
};
