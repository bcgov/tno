import { IReportResultModel } from 'tno-core';

export interface IReportResultForm extends IReportResultModel {
  instanceId?: number;
  /** Actionable size validation failure for this instance's preview. */
  sizeError?: string;
}
