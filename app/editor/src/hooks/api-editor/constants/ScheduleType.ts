export enum ScheduleType {
  /** Repeating will run a continuously repeating schedule */
  Continuous = 0,
  /** Managed will run between a start and stop time */
  Daily = 1,
  /** Runs based on a programmed schedule of multiple start ands top events. */
  Advanced = 2,
}
