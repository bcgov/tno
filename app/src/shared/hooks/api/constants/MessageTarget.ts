export enum MessageTarget {
  /** Content messages */
  ContentAdded = 0,
  /** Content messages */
  ContentUpdated = 1,
  /** Content messages */
  ContentDeleted = 2,
  /** Work order messages */
  WorkOrder = 3,
  /** Content action message */
  ContentActionUpdated = 4,
  /** Ingest updated */
  IngestUpdated = 5,
  /** Ingest deleted */
  IngestDeleted = 6,
  /** Report status updated */
  ReportStatus = 7,
  /** Request for the user to be logged out automatically */
  Logout = 8,
  /** A system message. */
  SystemMessage = 9,
}
