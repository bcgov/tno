import { formatDashboardDate, INotificationInstanceModel } from 'tno-core';

export const getLastSent = (instance: INotificationInstanceModel) => {
  const sentOn = instance?.sentOn;
  if (!sentOn) return 'Never';
  return formatDashboardDate(sentOn, true);
};
