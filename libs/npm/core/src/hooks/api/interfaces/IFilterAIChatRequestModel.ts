import { IFilterAIMessageModel, IFilterSettingsModel } from '.';

export interface IFilterAIChatRequestModel {
  question: string;
  /** The draft prompt (HTML). */
  prompt?: string;
  settings?: IFilterSettingsModel;
  /** The explanation from the last generation. */
  explanation?: string;
  /** The conversation nonce returned by the last answer. */
  nonce?: string;
  /** The conversation returned by the last answer. */
  messages: IFilterAIMessageModel[];
}
