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
    name: keyof T,
    label: string,
    cell: ((cell: ITableInternalCell<T>) => React.ReactNode) | undefined,
    original: T,
    options: {
      isVisible?: boolean;
      hAlign?: 'left' | 'center' | 'right';
      vAlign?: 'center' | 'top' | 'bottom';
      width?: string | number;
    },
  ) {
    super(index, name, label, cell, options);
    this.row = row;
    this.original = original;
  }
}
