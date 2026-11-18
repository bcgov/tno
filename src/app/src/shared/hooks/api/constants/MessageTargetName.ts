export enum MessageTargetName {
  /** Content messages */
  ContentAdded = 'ContentAdded',
  /** Content messages */
  ContentUpdated = 'ContentUpdated',
  /** Content messages */
  ContentDeleted = 'ContentDeleted',
  /** Work order messages */
  WorkOrder = 'WorkOrder',
  /** Content action message */
  ContentActionUpdated = 'ContentActionUpdated',
  /** Ingest updated */
  IngestUpdated = 'IngestUpdated',
  /** Ingest deleted */
  IngestDeleted = 'IngestDeleted',
  /** Report status updated */
  ReportStatus = 'ReportStatus',
  /** Request for the user to be logged out automatically */
  Logout = 'Logout',
  /** A system message. */
  SystemMessage = 'SystemMessage',
}
