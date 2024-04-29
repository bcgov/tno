import { ReportStatusName } from 'tno-core';

/**
 * Provides a message that represents the status.
 * @param status The report status.
 * @returns A message that represents the status.
 */
export const getStatus = (status: ReportStatusName) => {
  switch (status) {
    case ReportStatusName.Pending:
      return 'Draft';
    case ReportStatusName.Submitted:
      return 'Sending';
    case ReportStatusName.Failed:
      return 'Failed to Send';
    case ReportStatusName.Accepted:
    case ReportStatusName.Completed:
    default:
      return 'Sent';
  }
};
