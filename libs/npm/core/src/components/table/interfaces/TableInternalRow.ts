import {
  ITableInternal,
  ITableInternalCell,
  ITableInternalColumn,
  ITableInternalRow,
  TableInternalCell,
} from '.';

export class TableInternalRow<T extends object> implements ITableInternalRow<T> {
  table: ITableInternal<T>;
  index: number;
  rowId: keyof T;
  original: T;
  isSelected: boolean;
  isActive: boolean;
  cells: ITableInternalCell<T>[];
  toggleSelected: (value?: boolean) => void;

  constructor(
    table: ITableInternal<T>,
    index: number,
    rowId: keyof T,
    columns: ITableInternalColumn<T>[],
    original: T,
    isSelected: boolean,
  ) {
    this.table = table;
    this.index = index;
    this.rowId = rowId;
    this.original = original;
    this.isSelected = isSelected;
    this.isActive = table.activeRow
      ? table.activeRow?.original?.[rowId] === original[rowId]
      : table.activeRowId === `${original[rowId]}`;
    this.cells = columns.map(
      (col) =>
        new TableInternalCell(this, col.index, col.name, col.label, col.cell, original, {
          isVisible: col.isVisible,
        }),
    );
    this.toggleSelected = (value?: boolean) =>
      value === undefined ? (this.isSelected = !this.isSelected) : value;
  }
}
