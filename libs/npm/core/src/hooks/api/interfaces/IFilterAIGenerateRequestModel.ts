import { IFilterSettingsModel } from '.';

export interface IFilterAIGenerateRequestModel {
  /** The prompt describing the filter (HTML). */
  prompt: string;
  /** The current settings; their index, size, paging and sort are kept. */
  settings?: IFilterSettingsModel;
}
