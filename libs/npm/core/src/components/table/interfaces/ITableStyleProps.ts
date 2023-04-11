import { ITableColumn } from '.';

export interface ITableStyleProps {
  /** The table class name */
  className?: string;
  /** Control the table rows and vertical scrolling */
  scrollSize?: number | string;
  /** An array of columns */
  columns: ITableColumn[];
}
