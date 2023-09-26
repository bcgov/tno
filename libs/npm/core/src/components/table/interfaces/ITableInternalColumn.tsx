import { ITableHookColumn, ITableInternalCell } from '.';

export interface ITableInternalColumn<T extends object>
  extends Omit<ITableHookColumn<T>, 'id' | 'sort' | 'showSort' | 'isSorted' | 'isSortedDesc'> {
  index: number;
  name: keyof T;
  isVisible: boolean;
  cell: (cell: ITableInternalCell<T>) => React.ReactNode;
}

export class TableInternalColumn<T extends object> implements ITableInternalColumn<T> {
  index: number;
  name: keyof T;
  label: React.ReactNode; // TODO: Need to separate header columns and row columns.  Also need to make this a function
  cell: (cell: ITableInternalCell<T>) => React.ReactNode;
  isVisible: boolean;
  hAlign?: 'left' | 'center' | 'right';
  vAlign?: 'center' | 'top' | 'bottom';
  width?: string | number;

  constructor(
    index: number,
    name: keyof T,
    label: React.ReactNode,
    cell: ((cell: ITableInternalCell<T>) => React.ReactNode) | undefined,
    options: {
      isVisible?: boolean;
      hAlign?: 'left' | 'center' | 'right';
      vAlign?: 'center' | 'top' | 'bottom';
      width?: string | number;
    } = {
      isVisible: true,
    },
  ) {
    this.index = index;
    this.name = name;
    this.label = label;
    this.cell = cell ? cell : (cell) => <>{cell.original[name]}</>;
    this.isVisible = options.isVisible ?? true;
    this.hAlign = options?.hAlign;
    this.vAlign = options?.vAlign;
    this.width = options?.width;
  }
}
