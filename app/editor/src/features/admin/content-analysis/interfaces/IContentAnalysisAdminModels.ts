/** The Content-Analysis settings (the processes it runs are configured on the service). */
export interface IContentAnalysisSettingsModel {
  llmId?: number;
  excludedMediaTypeIds: number[];
  excludedSourceIds: number[];
}

/** Analysis job counts keyed 'Status:Reason', plus 'Due'. */
export interface IAnalysisQueueModel {
  counts: Record<string, number>;
}

export type AnalysisBackfillDateFieldName = 'PublishedOn' | 'CreatedOn';
export type AnalysisBackfillModeName = 'MissingOrStale' | 'Force';

/** The criteria of a backfill; start inclusive, end exclusive, both UTC. */
export interface IAnalysisBackfillRequestModel {
  startOn: string;
  endOn: string;
  timeZone: string;
  dateField: AnalysisBackfillDateFieldName;
  mode: AnalysisBackfillModeName;
}

/** What a backfill would cover. */
export interface IAnalysisBackfillPreviewModel {
  matching: number;
  excluded: number;
  missingPublicationDate: number;
  alreadyCurrent: number;
}

/** A backfill and its progress. */
export interface IAnalysisBackfillModel extends IAnalysisBackfillRequestModel {
  id: number;
  status: 'Pending' | 'Running' | 'Completed' | 'Failed' | 'Cancelled';
  total: number;
  scheduled: number;
  alreadyCurrent: number;
  analyzed: number;
  superseded: number;
  deleted: number;
  failed: number;
  remaining: number;
  indexed: number;
  isComplete: boolean;
  error?: string;
  createdBy: string;
  createdOn?: string;
  completedOn?: string;
}
