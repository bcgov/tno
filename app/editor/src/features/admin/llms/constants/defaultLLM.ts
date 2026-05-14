import { type ILLMModel } from 'tno-core';

export const defaultLLM: ILLMModel = {
  id: 0,
  name: '',
  description: '',
  isEnabled: true,
  sortOrder: 0,
  deploymentName: '',
  agentName: '',
  isPublic: false as unknown as string,
  systemPrompt: '',
  userPrompt: '',
  minTemperature: undefined,
  maxTemperature: undefined,
  apiKey: '',
  projectEndpoint: '',
};
