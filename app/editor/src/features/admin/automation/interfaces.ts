export interface IAutomationScheduleModel {
  /** The event schedule id (0 when new). */
  id: number;
  /** A name to identify the schedule. */
  name: string;
  isEnabled: boolean;
  /** Time of day to run at (or after), 'HH:mm:ss'. */
  startAt?: string | null;
  /** The date/time the schedule becomes valid; the scheduler will not fire before it. Set it in
   * the future so a schedule created after its 'startAt' time has passed does not run prematurely
   * on the day it is created. */
  runOn?: string | null;
  /** ScheduleWeekDay flag values; empty runs every day. */
  runOnWeekDays: number[];
}

export interface IAutomationProfileModel {
  id: number;
  name: string;
  description: string;
  isEnabled: boolean;
  schemaVersion: number;
  /** The definition document as raw JSON (prompts library, phased steps, analyses, actions). */
  definition?: string | null;
  llmId?: number;
  schedules: IAutomationScheduleModel[];
}

export interface IAutomationRunRequestModel {
  note?: string;
  /** Compute and log every decision and change without writing anything. */
  isDryRun?: boolean;
  /** A candidate definition (raw v2 JSON) for a comparison run; forces a dry run. */
  compareDefinition?: string | null;
}

/** A single message in a debugging conversation. */
export interface IAutomationDebugMessageModel {
  role: string;
  content: string;
}

/** A request to ask the profile's LLM why a content item was (or was not) acted upon. */
export interface IAutomationDebugRequestModel {
  /** Optional focus item; 0/absent answers against the most recent run as a whole. */
  contentId?: number;
  question: string;
  /** The conversation returned from the previous response; empty starts a new chat. */
  messages?: IAutomationDebugMessageModel[];
}

/** The LLM's answer to a debugging turn, plus the full conversation to continue it. */
export interface IAutomationDebugResultModel {
  contentId: number;
  runId?: number;
  prompt: string;
  answer: string;
  messages: IAutomationDebugMessageModel[];
}

export interface IAutomationRunModel {
  id: number;
  profileId?: number;
  status?: string | number;
  trigger?: string;
  note?: string;
  startedOn?: string;
  completedOn?: string;
  /** Whether the run computed and logged everything but wrote nothing. */
  isDryRun?: boolean;
  /** The run outcome summary JSON (engineVersion 2). */
  summary?: string | null;
}

export interface IAutomationRunDiffModel {
  run: IAutomationRunModel;
  changes: unknown[];
  stepHits: unknown[];
}
