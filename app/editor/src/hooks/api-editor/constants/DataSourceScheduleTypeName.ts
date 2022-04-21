export enum DataSourceScheduleTypeName {
  /** This data source has no schedule. */
  None = 'None',
  /** Repeating will run a continuously repeating schedule. */
  Continuous = 'Continuous',
  /** Has a start and stop time each day. */
  Daily = 'Daily',
  /** Advanced schedule has many start and stop times. */
  Advanced = 'Advanced',
}
