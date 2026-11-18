import { IReportAISectionStatusModel } from './IReportAISectionStatusModel';

export interface IReportResultModel {
  reportId: number;
  instanceId?: number;
  subject: string;
  body: string;
  data?: any;
  aiSections?: IReportAISectionStatusModel[];
}
