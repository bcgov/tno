import { Checkbox } from '../../form';
import {
  getSortId,
  ITableHookColumn,
  ITableInternalCell,
  ITableInternalRow,
  ITableSort,
  TableHookColumn,
  TableInternal,
} from '..';

/**
 * Applies sort rules to the specified columns.
 * @param table The table.
 * @param columns An array of columns.
 * @param sortOrder An array of sort rules.
 * @returns An updated array of columns.
 */
export const initColumns = <T extends object>(
  table: TableInternal<T>,
  columns: ITableHookColumn<T>[],
  sortOrder: ITableSort<T>[] | undefined = [],
) => {
  const cols = [...columns];
  if (table.options.isMulti)
    cols.unshift(
      new TableHookColumn<T>(
        undefined,
        '',
        (cell: ITableInternalCell<T>) => {
          return (
            <Checkbox
              name={`select-${cell.original[table.rowId]}`}
              checked={cell.row.isSelected}
              onChange={(e) => {
                table.onSelectedChanged(cell.row, e);
              }}
              onClick={(e) => {
                e.stopPropagation();
              }}
            />
          );
        },
        {
          showSort: true,
          sort: (row: ITableInternalRow<T>) => {
            return row.isSelected;
          },
          hAlign: 'center',
          vAlign: 'center',
          width: '24px',
        },
      ),
    );

  return cols.map((col, index) => {
    const id = getSortId(col, index);
    sortOrder.some((sort) => {
      if (id === sort.id) {
        col.isSorted = sort.isSorted;
        col.isSortedDesc = sort.isSortedDesc;
        return sort.isSorted;
      }
      return false;
    });
    return col;
  });
};
