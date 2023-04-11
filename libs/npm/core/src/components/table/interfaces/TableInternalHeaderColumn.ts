import { ITableInternal, ITableInternalHeaderColumn, ITableInternalRow } from '.';

export class TableInternalHeaderColumn<T extends object> implements ITableInternalHeaderColumn<T> {
  index: number;
  name: keyof T | string;
  label: string; // TODO: Need to separate header columns and row columns.  Also need to make this a function
  isVisible: boolean;
  sort: string | ((row: ITableInternalRow<T>) => string | number | boolean | null | undefined);
  showSort: boolean;
  isSorted: boolean;
  isSortedDesc: boolean;
  hAlign?: 'left' | 'center' | 'right';
  vAlign?: 'center' | 'top' | 'bottom';
  width?: string | number;

  constructor(
    table: ITableInternal<T>,
    index: number,
    name: keyof T | string,
    label: string,
    isVisible: boolean,
    sort?:
      | string
      | ((row: ITableInternalRow<T>) => string | number | boolean | null | undefined)
      | undefined,
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
    this.name = name;
    this.label = label;
    this.sort = sort ?? name.toString();
    this.isVisible = isVisible ?? true;
    this.showSort = showSort ?? table.showSort ?? false;
    this.isSorted = isSorted ?? false;
    this.isSortedDesc = isSortedDesc ?? false;
    this.hAlign = options?.hAlign;
    this.vAlign = options?.vAlign;
    this.width = options?.width;
  }
}
