export interface IPage<T> {
  /** Page index (zero-based). */
  pageIndex: number;
  /** Number of items per page. */
  pageSize: number;
  /** Total number of pages. */
  pageCount: number;
  /** Array of items on page. */
  items: T[];
  /** Total number of items in source. */
  total?: number;
}
