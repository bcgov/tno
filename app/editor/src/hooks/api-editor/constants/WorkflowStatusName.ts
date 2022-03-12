export enum WorkflowStatusName {
  /**
   * Content has failed to be added to TNO.
   */
  Failed = 'Failed',
  /**
   * Content has been received from data source and is in progress of being
   * ingested. It has not yet been added to TNO.
   */
  InProgress = 'InProgress',
  /**
   * Content has been received by TNO, but is not searchable.
   */
  Received = 'Received',
  /**
   * Content has been received and transcribed in TNO but is not yet searchable.
   */
  Transcribed = 'Transcribed',
  /**
   * Content has been received and Natural Language Processed in TNO but is not
   * yet searchable.
   */
  NLP = 'NLP',
  /**
   * Content has successfully been added to TNO, and is searchable.
   */
  Success = 'Success',
  /**
   * Content has been published.
   */
  Published = 'Published',
  /**
   * Content has been unpublished.
   */
  Unpublished = 'Unpublished',
}
