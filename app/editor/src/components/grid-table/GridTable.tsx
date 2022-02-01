import { Column, Row, useTable } from 'react-table';

import * as styled from './GridTableStyled';

/**
 * GridTable properties.
 */
export interface IGridTableProps<CT extends object = Record<string, unknown>> {
  /**
   * An array of column definitions.
   */
  columns: Column<CT>[];
  /**
   * An array of row data.
   */
  data: CT[];
  /**
   * Handle row click event.
   */
  onRowClick?: (row: Row<CT>) => void;
}

/**
 * GridTable component provides a way to list and filter data in a grid table.
 * @param param1 GridTable properties.
 * @returns GridTable component.
 */
export const GridTable = <T extends object>({ columns, data, onRowClick }: IGridTableProps<T>) => {
  const { getTableProps, getTableBodyProps, headerGroups, rows, prepareRow } = useTable({
    columns,
    data,
  });

  return (
    <styled.GridTable {...getTableProps()}>
      <div role="rowheader">
        {headerGroups.map((headerGroup) => (
          <div {...headerGroup.getHeaderGroupProps()}>
            {headerGroup.headers.map((column) => (
              <div {...column.getHeaderProps()}>{column.render('Header')}</div>
            ))}
          </div>
        ))}
      </div>
      <div {...getTableBodyProps()}>
        {rows.map((row) => {
          prepareRow(row);
          return (
            <div {...row.getRowProps()} onClick={() => onRowClick && onRowClick(row)}>
              {row.cells.map((cell) => {
                return <div {...cell.getCellProps()}>{cell.render('Cell')}</div>;
              })}
            </div>
          );
        })}
      </div>
    </styled.GridTable>
  );
};
