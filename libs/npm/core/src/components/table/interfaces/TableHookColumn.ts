import { ITableHookColumn, ITableInternalCell, ITableInternalRow } from '.';

export class TableHookColumn<T extends object> implements ITableHookColumn<T> {
  name: keyof T | string;
  label: string;
  cell?: ((cell: ITableInternalCell<T>) => React.ReactNode) | undefined;
  isVisible: boolean;
  sort: string | ((row: ITableInternalRow<T>) => string | number | boolean | undefined | null);
  showSort: boolean;
  isSorted: boolean;
  isSortedDesc: boolean;
  hAlign?: 'left' | 'center' | 'right';
  vAlign?: 'top' | 'center' | 'bottom';
  width?: string | number;

  constructor(
    name: keyof T | string,
    label: string,
    cell?: ((column: ITableInternalCell<T>) => React.ReactNode) | undefined,
    options: {
      isVisible?: boolean;
      sort?: string | ((row: ITableInternalRow<T>) => string | number | boolean | undefined | null);
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
    this.name = name;
    this.label = label;
    this.cell = cell;
    this.isVisible = options?.isVisible ?? true;
    this.sort = options?.sort ?? name.toString();
    this.showSort = options?.showSort ?? false;
    this.isSorted = options?.isSorted ?? false;
    this.isSortedDesc = options?.isSortedDesc ?? false;
    this.hAlign = options?.hAlign;
    this.vAlign = options?.vAlign;
    this.width = options?.width;
  }
}
