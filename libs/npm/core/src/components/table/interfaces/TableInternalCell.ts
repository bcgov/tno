import { ITableInternalCell, ITableInternalRow, TableInternalColumn } from '.';

export class TableInternalCell<T extends object>
  extends TableInternalColumn<T>
  implements ITableInternalCell<T>
{
  row: ITableInternalRow<T>;
  original: T;

  public constructor(
    row: ITableInternalRow<T>,
    index: number,
    accessor: keyof T | string | undefined | ((data: T) => unknown),
    label: React.ReactNode,
    cell: ((cell: ITableInternalCell<T>) => React.ReactNode) | undefined,
    original: T,
    options: {
      isVisible?: boolean;
      hAlign?: 'left' | 'center' | 'right';
      vAlign?: 'center' | 'top' | 'bottom';
      width?: string | number;
    },
  ) {
    super(index, accessor, label, cell, options);
    this.row = row;
    this.original = original;
  }
}
