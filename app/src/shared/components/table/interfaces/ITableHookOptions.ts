import { IdType, ITableInternalCell, ITableInternalColumn, ITableInternalRow } from '.';

export interface ITableHookOptions<T extends object> {
  onRowClick?: (row: ITableInternalRow<T>, event: React.MouseEvent) => void;
  onColumnClick?: (column: ITableInternalColumn<T>, event: React.MouseEvent) => void;
  onCellClick?: (
    cell: ITableInternalCell<T>,
    row: ITableInternalRow<T>,
    event: React.MouseEvent,
  ) => void;
  onKeyDown?: (event: React.KeyboardEvent<HTMLDivElement>) => void;
  onSelectedChanged?: (
    row: ITableInternalRow<T>,
    event: React.ChangeEvent<HTMLInputElement>,
  ) => void;
  stopPropagation?: boolean;
  showHeader?: boolean;
  showFooter?: boolean;
  isMulti?: boolean;
  selectedRowIds?: IdType<T>[];
  activeRowId?: IdType<T>;
}
