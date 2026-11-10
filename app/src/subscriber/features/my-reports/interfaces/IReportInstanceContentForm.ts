import { IReportInstanceContentModel } from '@/shared';

export interface IReportInstanceContentForm extends IReportInstanceContentModel {
  originalIndex: number;
  /** Whether this row has been selected */
  selected?: boolean;
}
