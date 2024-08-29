import { NotificationStatusName } from 'tno-core';

/**
 * Provides a message that represents the status.
 * @param status The report status.
 * @returns A message that represents the status.
 */
export const getStatus = (status?: NotificationStatusName) => {
  if (!status) return 'Pending';
  switch (status) {
    case NotificationStatusName.Failed:
      return 'Failed to Send';
    case NotificationStatusName.Cancelled:
      return 'Cancelled';
    case NotificationStatusName.Accepted:
    case NotificationStatusName.Completed:
      return 'Sent';
    case NotificationStatusName.Pending:
    default:
      return 'Pending';
  }
};
