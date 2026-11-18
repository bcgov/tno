/** The rows a history purge deleted, or would delete for a dry run, per table. */
export interface IHistoryPurgeModel {
  /** Whether the purge was a dry run that deleted nothing. */
  dryRun: boolean;
  /** The retention in days that was applied. Zero means the purge is disabled. */
  retentionDays: number;
  /** Rows per table, keyed by table name. */
  tables: Record<string, number>;
}

/** The dry-run counts of both history purges. */
export interface IHistoryPurgePreviewModel {
  reports: IHistoryPurgeModel;
  notifications: IHistoryPurgeModel;
}
