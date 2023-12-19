import { IReportInstanceContentModel } from 'tno-core';

export interface IReportInstanceContentForm extends IReportInstanceContentModel {
  originalIndex: number;
  /** Whether this row has been selected */
  selected?: boolean;
}
