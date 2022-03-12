export enum WorkflowStatus {
  /**
   * Content has failed to be added to TNO.
   */
  Failed = -1,
  /**
   * Content has been received from data source and is in progress of being
   * ingested. It has not yet been added to TNO.
   */
  InProgress = 0,
  /**
   * Content has been received by TNO, but is not searchable.
   */
  Received = 1,
  /**
   * Content has been received and transcribed in TNO but is not yet searchable.
   */
  Transcribed = 2,
  /**
   * Content has been received and Natural Language Processed in TNO but is not
   * yet searchable.
   */
  NLP = 3,
  /**
   * Content has successfully been added to TNO, and is searchable.
   */
  Success = 4,
  /**
   * Content has been published.
   */
  Published = 5,
  /**
   * Content has been unpublished.
   */
  Unpublished = 6,
}
