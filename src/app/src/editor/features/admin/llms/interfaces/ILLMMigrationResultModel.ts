import { type ILLMUsageModel } from './ILLMUsageModel';

export interface ILLMMigrationResultModel {
  migrated: number;
  temperaturesChanged: number;
  usage: ILLMUsageModel;
}
