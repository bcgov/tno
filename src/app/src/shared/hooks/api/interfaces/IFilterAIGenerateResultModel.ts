import { IFilterAIStepModel, IFilterSettingsModel } from '.';

export interface IFilterAIGenerateResultModel {
  /** The generated settings, including any extended query. */
  settings: IFilterSettingsModel;
  /** How the settings meet the prompt. */
  explanation: string;
  /** The tools the model used, in order. */
  steps: IFilterAIStepModel[];
}
