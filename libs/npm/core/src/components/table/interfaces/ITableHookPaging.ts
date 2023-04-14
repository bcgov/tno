import { ITableInternal, ITablePage } from '.';

export interface ITableHookPaging<T extends object> {
  /** The current page index position (default=0) */
  pageIndex?: number;
  /** The current quantity of items to show on a page (default=10) */
  pageSize?: number;
  /** Manually control the number of pages available */
  pageCount?: number;
  /** Whether you will manually control the paging (default=false) */
  manualPaging?: boolean;
  /** The number of paging number buttons that are displayed (default=5) */
  pageButtons?: number;
  /** Whether to show the paging footer (default=true) */
  showPaging?: boolean;
  /** An event that fires when the page changes */
  onPageChange?: (page: ITablePage, table: ITableInternal<T>) => void;
  /** Control the table rows and vertical scrolling */
  scrollSize?: number | string;
}
