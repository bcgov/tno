export enum DataSourceScheduleType {
  /** This data source has no schedule. */
  None = 0,
  /** Repeating will run a continuously repeating schedule. */
  Continuous = 1,
  /** Has a start and stop time each day. */
  Daily = 2,
  /** Advanced schedule has many start and stop times. */
  Advanced = 3,
}
