import { ITableInternal } from './ITableInternal';
import { ITableInternalHeaderColumn } from './ITableInternalHeaderColumn';
import { ITableInternalRow } from './ITableInternalRow';

export class TableInternalHeaderColumn<T extends object> implements ITableInternalHeaderColumn<T> {
  index: number;
  accessor: keyof T | string | undefined | ((data: T) => unknown);
  label: React.ReactNode; // TODO: Need to separate header columns and row columns.  Also need to make this a function
  isVisible: boolean;
  sort:
    | keyof T
    | string
    | undefined
    | ((row: ITableInternalRow<T>) => string | number | boolean | null | undefined);
  showSort: boolean;
  isSorted: boolean;
  isSortedDesc: boolean;
  hAlign?: 'left' | 'center' | 'right';
  vAlign?: 'center' | 'top' | 'bottom';
  width?: string | number;

  constructor(
    table: ITableInternal<T>,
    index: number,
    accessor: keyof T | string | undefined | ((data: T) => unknown),
    label: React.ReactNode,
    isVisible: boolean,
    sort?:
      | keyof T
      | string
      | ((row: ITableInternalRow<T>) => string | number | boolean | null | undefined),
    showSort?: boolean | undefined,
    isSorted?: boolean | undefined,
    isSortedDesc?: boolean | undefined,
    options?: {
      hAlign?: 'left' | 'center' | 'right';
      vAlign?: 'center' | 'top' | 'bottom';
      width?: string | number;
    },
  ) {
    this.index = index;
    this.accessor = accessor;
    this.label = label;
    this.sort = sort;
    this.isVisible = isVisible ?? true;
    this.showSort = showSort ?? table.showSort ?? false;
    this.isSorted = isSorted ?? false;
    this.isSortedDesc = isSortedDesc ?? false;
    this.hAlign = options?.hAlign;
    this.vAlign = options?.vAlign;
    this.width = options?.width;
  }
}
