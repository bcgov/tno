import { IReportSectionSettingsModel } from 'tno-core';

/** One message of a request to a model. */
export interface ILlmMessageModel {
  role: string;
  content: string;
}

/** One request an AI section sent (or would send) to its model and what came back. */
export interface ILlmExchangeModel {
  /** The display label of the step, e.g. "Map batch 2/5", "Reduce round 1", "Final". */
  stage: string;
  /** Map, Reduce, Final, HistoryMap, HistoryReduce or Agent. */
  kind: string;
  /** The send order, from 1. */
  sequence: number;
  messages: ILlmMessageModel[];
  /** The response text; absent when planned or failed. */
  response?: string;
  promptTokens?: number;
  completionTokens?: number;
  durationMs: number;
  jsonMode: boolean;
  isTruncated: boolean;
  error?: string;
  estimatedTokens: number;
}

/** The requests an AI section would send for the report's current content, described without sending any. */
export interface IReportAISectionPromptModel {
  reportId: number;
  reportInstanceId?: number;
  sectionId: number;
  sectionLabel: string;
  llmName?: string;
  /** True: the exchanges were planned, not sent, so they carry no responses. */
  isPlanned: boolean;
  exchanges: ILlmExchangeModel[];
  error?: string;
}

export type ReportAISectionTestStatusName = 'Pending' | 'Completed' | 'Failed';

/** The outcome of a test run of an AI section. */
export interface IReportAISectionTestResultModel {
  testId: string;
  reportId: number;
  reportInstanceId: number;
  sectionId: number;
  requestorId?: number;
  status: ReportAISectionTestStatusName;
  output?: string;
  error?: string;
  requestCount: number;
  promptTokens: number;
  completionTokens: number;
  durationMs: number;
  storyCount: number;
  reductionDepth: number;
  transcript: ILlmExchangeModel[];
  createdOn: string;
  completedOn?: string;
}

/** One message in a conversation about an AI section prompt. */
export interface IReportAIPromptChatMessageModel {
  role: string;
  content: string;
}

/** A turn of a conversation with the section's model about how to change the section's prompt. */
export interface IReportAIPromptChatRequestModel {
  settings: IReportSectionSettingsModel;
  question: string;
  /** The conversation so far; empty starts a new one. */
  messages: IReportAIPromptChatMessageModel[];
  /** A test run whose transcript and output give the conversation its context. */
  testId?: string;
}

/** The model's answer, a prompt it proposes when it does, and the conversation to send back next. */
export interface IReportAIPromptChatResultModel {
  prompt: string;
  answer: string;
  suggestedPrompt?: string;
  messages: IReportAIPromptChatMessageModel[];
}
