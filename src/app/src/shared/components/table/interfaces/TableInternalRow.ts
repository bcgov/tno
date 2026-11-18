import { ITableInternal } from './ITableInternal';
import { ITableInternalCell } from './ITableInternalCell';
import { ITableInternalColumn } from './ITableInternalColumn';
import { ITableInternalRow } from './ITableInternalRow';
import { TableInternalCell } from './TableInternalCell';

export class TableInternalRow<T extends object> implements ITableInternalRow<T> {
  table: ITableInternal<T>;
  index: number;
  rowId: keyof T | ((data?: T) => string);
  _rowId: (data?: T) => string;
  original: T;
  isSelected: boolean;
  isActive: boolean;
  cells: ITableInternalCell<T>[];
  columns: ITableInternalColumn<T>[];
  toggleSelected: (value?: boolean) => void;

  constructor(
    table: ITableInternal<T>,
    index: number,
    rowId: keyof T | ((data?: T) => string),
    columns: ITableInternalColumn<T>[],
    original: T,
    isSelected: boolean,
  ) {
    this.table = table;
    this.index = index;
    this.rowId = rowId;
    this._rowId = typeof rowId === 'function' ? rowId : (data?: T) => `${data?.[rowId]}`;
    this.original = original;
    this.isSelected = isSelected;
    this.isActive = table.activeRow
      ? this._rowId(table.activeRow?.original) === this._rowId(original)
      : table.activeRowId === this._rowId(original);
    this.columns = columns;
    this.cells = columns.map(
      (col) =>
        new TableInternalCell(this, col.index, col.accessor, col.label, col.cell, original, {
          isVisible: col.isVisible,
        }),
    );
    this.toggleSelected = (value?: boolean) =>
      value === undefined ? (this.isSelected = !this.isSelected) : value;
  }
}
