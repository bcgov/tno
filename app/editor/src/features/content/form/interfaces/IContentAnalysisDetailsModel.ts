/** Who set a populated editorial value. */
export type FieldOwnerName = 'Human' | 'Analysis' | 'Automation';

/** The state of a content item's analysis work. */
export type AnalysisJobStatusName = 'Pending' | 'Claimed' | 'Completed' | 'Failed' | 'Skipped';

/** A content item's analysis job. */
export interface IAnalysisJobModel {
  id: number;
  contentId: number;
  reason: 'Lifecycle' | 'Reanalysis' | 'Backfill';
  status: AnalysisJobStatusName;
  attempts: number;
  dueOn: string;
  lastError?: string;
  updatedOn?: string;
  backfillId?: number;
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

/** A content item's analysis, field ownership, and queued work. */
export interface IContentAnalysisDetailsModel {
  analysis?: IContentAnalysisModel;
  ownership: IContentFieldOwnershipModel[];
  job?: IAnalysisJobModel;
}
