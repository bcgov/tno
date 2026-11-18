export enum WorkflowStatusName {
  /** Content has failed to be added to TNO. */
  Failed = 'Failed',

  /** Content has been discovered and is currently being ingested. */
  InProgress = 'InProgress',

  /** Content has been ingested, files have been copied, and message posted to Kafka. */
  Received = 'Received',

  /** Content has been imported into the TNO database, and files have been copied into location API has access to. */
  Imported = 'Imported',
}
