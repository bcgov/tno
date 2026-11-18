export interface ITableGroup<T> {
  /** Unique key to group each collection of rows. */
  key: string;
  /** An array of rows that belong to this group. */
  rows: T[];
}
