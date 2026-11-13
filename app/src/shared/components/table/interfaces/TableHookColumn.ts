import { ITableHookColumn, ITableInternalCell, ITableInternalRow } from '.';

export class TableHookColumn<T extends object> implements ITableHookColumn<T> {
  accessor: keyof T | undefined | string | ((data: T) => unknown);
  label: string;
  cell?: ((cell: ITableInternalCell<T>) => React.ReactNode) | undefined;
  isVisible: boolean;
  sort:
    | keyof T
    | undefined
    | string
    | ((row: ITableInternalRow<T>) => string | number | boolean | undefined | null);
  showSort: boolean;
  isSorted: boolean;
  isSortedDesc: boolean;
  hAlign?: 'left' | 'center' | 'right';
  vAlign?: 'top' | 'center' | 'bottom';
  width?: string | number;

  constructor(
    accessor: keyof T | undefined | string | ((data: T) => unknown),
    label: string,
    cell?: ((column: ITableInternalCell<T>) => React.ReactNode) | undefined,
    options: {
      isVisible?: boolean;
      sort?:
        | keyof T
        | string
        | ((row: ITableInternalRow<T>) => string | number | boolean | undefined | null);
      showSort?: boolean;
      isSorted?: boolean;
      isSortedDesc?: boolean;
      hAlign?: 'left' | 'center' | 'right';
      vAlign?: 'top' | 'center' | 'bottom';
      width?: string | number;
    } = {
      isVisible: true,
    },
  ) {
    this.accessor = accessor;
    this.label = label;
    this.cell = cell;
    this.isVisible = options?.isVisible ?? true;
    this.sort = options?.sort;
    this.showSort = options?.showSort ?? false;
    this.isSorted = options?.isSorted ?? false;
    this.isSortedDesc = options?.isSortedDesc ?? false;
    this.hAlign = options?.hAlign;
    this.vAlign = options?.vAlign;
    this.width = options?.width;
  }
}
