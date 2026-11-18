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

/** The status of a bulk rescore's work order, as serialized by the API. */
export enum RescoreStatusName {
  Submitted = 'Submitted',
  InProgress = 'InProgress',
  Completed = 'Completed',
  Cancelled = 'Cancelled',
  Failed = 'Failed',
}

/** A bulk rescore (a work order the Event Handler runs) and its progress. */
export interface ITopicRescoreJobModel {
  id: number;
  status: RescoreStatusName;
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
  updatedOn?: string;
}
