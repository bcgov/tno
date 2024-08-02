import { IAVOverviewInstanceModel } from './IAVOverviewInstanceModel';
import { IReportModel } from './IReportModel';

export interface IReportDashboard {
  reports: IReportModel[];
  overviews: IAVOverviewInstanceModel[];
}
