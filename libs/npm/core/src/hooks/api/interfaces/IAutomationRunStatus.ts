export enum AutomationRunStatus {
  /** Run has been queued and is waiting to be executed. */
  Draft = 0,
  /** Run has been picked up by the automation service and is executing. */
  Running = 1,
  /** Run completed successfully. */
  Completed = 2,
  /** Run failed to complete. */
  Failed = 3,
}
