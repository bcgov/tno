import { type ILLMUsageItemModel } from './ILLMUsageItemModel';

export interface ILLMUsageModel {
  llmId: number;
  items: ILLMUsageItemModel[];
  contentAnalysisResultCount: number;
}
