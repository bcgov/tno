/** The content a bulk rescore covers. */
export interface ITopicRescoreRequestModel {
  /** ISO date, inclusive. */
  startOn: string;
  /** ISO date, exclusive. */
  endOn: string;
  sourceIds: number[];
}

/** What a bulk rescore would change. */
export interface ITopicRescorePreviewModel {
  total: number;
  changed: number;
}

/** BackgroundJobStatus, as serialized by the API. */
export enum BackgroundJobStatusName {
  Pending = 'Pending',
  Running = 'Running',
  Completed = 'Completed',
  Failed = 'Failed',
  Cancelled = 'Cancelled',
}

/** A bulk rescore and its progress. */
export interface ITopicRescoreJobModel {
  id: number;
  status: BackgroundJobStatusName;
  startOn: string;
  endOn: string;
  sourceIds: number[];
  total: number;
  processed: number;
  changed: number;
  failed: number;
  error?: string;
  createdBy: string;
  createdOn?: string;
  startedOn?: string;
  completedOn?: string;
}
