import { ReportDistributionFormatName } from '../constants';
import { IUserModel } from './IUserModel';

export interface IUserReportModel extends IUserModel {
  /** Whether the user is subscribed to this report. */
  isSubscribed: boolean;
  /** Which distribution format they want to receive. */
  format: ReportDistributionFormatName;
}
