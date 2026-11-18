import { IFilterAIMessageModel, IFilterSettingsModel } from '.';

export interface IFilterAIChatRequestModel {
  question: string;
  /** The draft prompt (HTML). */
  prompt?: string;
  settings?: IFilterSettingsModel;
  /** The explanation from the last generation. */
  explanation?: string;
  /** The id of a listed story the question is about. */
  contentId?: number;
  /** The query the filter runs, so the answer can say why a story matched it. */
  query?: any;
  /** The conversation nonce returned by the last answer. */
  nonce?: string;
  /** The conversation returned by the last answer. */
  messages: IFilterAIMessageModel[];
}
