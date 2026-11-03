import { type ILLMModel } from 'tno-core';

/**
 * An LLM connection with its function-calling capability. tno-core 1.0.48 adds
 * supportsFunctionCalling to ILLMModel; until the editor uses that release it is declared here.
 */
export type ILLMConnectionModel = ILLMModel & {
  /** Whether the deployment supports native function calling (tools). */
  supportsFunctionCalling?: boolean;
};
