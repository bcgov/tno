import { type IAnalysisRunModel } from 'features/content/form/interfaces';

/** The Content-Analysis settings (the processes it runs are configured on the service). */
export interface IContentAnalysisSettingsModel {
  llmId?: number;
  excludedMediaTypeIds: number[];
  excludedSourceIds: number[];
}

/** Analysis requests waiting in each Kafka topic, and content whose newest request failed. */
export interface IAnalysisQueueModel {
  lag: Record<string, number>;
  failed: number;
}

/** Content whose newest analysis request failed. */
export interface IAnalysisFailureModel {
  contentId: number;
  headline: string;
  run: IAnalysisRunModel;
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

/** The status of a backfill's work order. */
export type AnalysisBackfillStatusName =
  | 'Submitted'
  | 'InProgress'
  | 'Completed'
  | 'Cancelled'
  | 'Failed';

/** A backfill (a work order the Event Handler runs) and its progress. */
export interface IAnalysisBackfillModel extends IAnalysisBackfillRequestModel {
  id: number;
  status: AnalysisBackfillStatusName;
  total: number;
  scheduled: number;
  alreadyCurrent: number;
  failed: number;
  error?: string;
  createdBy: string;
  createdOn?: string;
  updatedOn?: string;
}
