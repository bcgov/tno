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
  /** The tokens the model reads and writes in one request. */
  contextWindow?: number;
  /** The most tokens reserved for a response. */
  maxOutputTokens?: number;
  /** How tokens are counted: 'Heuristic', 'o200k_base', or 'cl100k_base'. */
  tokenEstimation?: string;
  /** Requests allowed per minute; undefined is unlimited. */
  requestsPerMinute?: number;
  /** Tokens allowed per minute; undefined is unlimited. */
  tokensPerMinute?: number;
}
