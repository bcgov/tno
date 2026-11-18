export enum WorkflowStatus {
  /** Content has failed to be added to TNO. */
  Failed = -1,

  /** Content has been discovered and is currently being ingested. */
  InProgress = 0,

  /** Content has been ingested, files have been copied, and message posted to Kafka. */
  Received = 1,

  /** Content has been imported into the TNO database, and files have been copied into location API has access to. */
  Imported = 2,
}
