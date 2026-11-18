/** Who set a populated editorial value. */
export type FieldOwnerName = 'Human' | 'Analysis' | 'Automation';

/** Why a story was sent for analysis. */
export type AnalysisRequestReasonName = 'Lifecycle' | 'Reanalysis' | 'Backfill' | 'Replay';

/** The outcome of an analysis request. */
export type AnalysisRunStatusName = 'Completed' | 'Skipped' | 'Retrying' | 'Failed';

/** A request to analyze a story, as sent to Kafka. */
export interface IAnalysisRequestModel {
  requestId: string;
  contentId: number;
  inputHash: string;
  reason: AnalysisRequestReasonName;
  force: boolean;
  requestedOn: string;
  workOrderId?: number;
}

/** The outcome of one analysis request for a story; retries update their request's run. */
export interface IAnalysisRunModel {
  requestId: string;
  reason: AnalysisRequestReasonName;
  status: AnalysisRunStatusName;
  inputHash: string;
  requestedOn: string;
  finishedOn: string;
  attempts: number;
  workOrderId?: number;
  analysisId?: number;
  error?: string;
}

/** A content item's structured analysis. */
export interface IContentAnalysisModel {
  id: number;
  contentId: number;
  isCurrent: boolean;
  isMetadataOnly: boolean;
  model: string;
  versions: string;
  summary: string;
  keyFacts: { statement: string; isInferred?: boolean }[];
  entities: {
    type: string;
    name: string;
    aliases?: string[];
    roles?: string[];
    isAmbiguous?: boolean;
  }[];
  places: { name: string; role: string }[];
  topics: { label: string; relevance: number }[];
  primaryTopic?: string;
  staffTopic?: string;
  suggestedTags: string[];
  suggestedContributor?: string;
  events: { actor: string; action: string; date?: string; location?: string }[];
  quotes: { statement: string; speaker: string }[];
  validation: Record<string, unknown>;
  promptTokens: number;
  completionTokens: number;
  analyzedOn: string;
}

/** Who owns a populated editorial value; a value without a record is human-owned. */
export interface IContentFieldOwnershipModel {
  field: string;
  valueKey: string;
  owner: FieldOwnerName;
  isCleared: boolean;
}

/** A content item's analysis, field ownership, and recent analysis runs (newest request first). */
export interface IContentAnalysisDetailsModel {
  analysis?: IContentAnalysisModel;
  ownership: IContentFieldOwnershipModel[];
  runs: IAnalysisRunModel[];
}
