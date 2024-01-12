import { IAuditColumnsModel } from './IAuditColumnsModel';

export interface ITimeTrackingModel extends IAuditColumnsModel {
  id: number;
  contentId: number;
  userId: number;
  effort: number;
  activity: string;
}
