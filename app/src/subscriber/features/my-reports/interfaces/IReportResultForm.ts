import { IReportResultModel } from '@/shared';

export interface IReportResultForm extends IReportResultModel {
  instanceId?: number;
  /** Actionable size validation failure for this instance's preview. */
  sizeError?: string;
}
