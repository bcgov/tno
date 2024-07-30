export enum MessageTargetKey {
  /** Content messages */
  ContentAdded = 'content-added',
  /** Content messages */
  ContentUpdated = 'content-updated',
  /** Content messages */
  ContentDeleted = 'content-deleted',
  /** Work order messages */
  WorkOrder = 'work-order',
  /** Content action message */
  ContentActionUpdated = 'content-action-updated',
  /** Ingest updated */
  IngestUpdated = 'ingest-updated',
  /** Ingest deleted */
  IngestDeleted = 'ingest-deleted',
  /** Report status updated */
  ReportStatus = 'report-status',
  /** Request for the user to be logged out automatically */
  Logout = 'logout',
  /** A system message. */
  SystemMessage = 'system-message',
}
