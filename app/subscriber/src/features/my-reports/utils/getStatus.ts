import { ReportStatusName } from 'tno-core';

/**
 * Provides a message that represents the status.
 * @param status The report status.
 * @returns A message that represents the status.
 */
export const getStatus = (status?: ReportStatusName) => {
  switch (status) {
    case ReportStatusName.Submitted:
      return 'Sending';
    case ReportStatusName.Failed:
      return 'Failed to Send';
    case ReportStatusName.Cancelled:
      return 'Cancelled';
    case ReportStatusName.Accepted:
      return 'Accepted';
    case ReportStatusName.Completed:
      return 'Completed';
    case ReportStatusName.Reopen:
      return 'Reopened';
    case ReportStatusName.Pending:
    default:
      return 'Draft';
  }
};
