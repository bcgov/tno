import { type ITimeTrackingModel } from '@/shared';

export interface ITimeTrackingForm extends ITimeTrackingModel {
  userName: string;
  /** Position in the form's timeTrackings array (unsaved entries have no id). */
  index: number;
}
