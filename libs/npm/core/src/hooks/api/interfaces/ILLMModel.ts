import { ISortableModel } from '.';

export interface ILLMModel extends ISortableModel<number> {
  deploymentName: string;
  agentName?: string;
  isPublic: string;
  systemPrompt: string;
  userPrompt: string;
  minTemperature?: number;
  maxTemperature?: number;
  apiKey?: string;
  projectEndpoint?: string;
}
