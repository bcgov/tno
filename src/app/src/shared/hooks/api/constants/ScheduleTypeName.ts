export enum ScheduleTypeName {
  /** This schedule does not run. */
  None = 'None',
  /** Repeating will run a continuously repeating schedule */
  Continuous = 'Continuous',
  /** Managed will run between a start and stop time */
  Daily = 'Daily',
  /** Runs based on a programmed schedule of multiple start ands top events. */
  Advanced = 'Advanced',
}
