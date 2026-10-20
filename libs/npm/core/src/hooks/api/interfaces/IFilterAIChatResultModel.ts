import { IFilterAIMessageModel, IFilterAIStepModel } from '.';

export interface IFilterAIChatResultModel {
  answer: string;
  /** A complete replacement prompt as HTML, when the model proposed one. */
  suggestedPrompt?: string;
  nonce: string;
  messages: IFilterAIMessageModel[];
  steps: IFilterAIStepModel[];
}
