import { type ILLMConnectionModel } from '../interfaces';

export const defaultLLM: ILLMConnectionModel = {
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
  supportsFunctionCalling: false,
};
