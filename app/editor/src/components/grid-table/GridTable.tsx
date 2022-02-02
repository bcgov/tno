import React from 'react';
import { Column, Row, usePagination, useTable } from 'react-table';

import { Pager } from '.';
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
  /**
   * The page has changed.
   */
  onPageChange?: (pageIndex: number, pageSize?: number) => void;
  /**
   * Whether to show the footer.
   */
  showFooter?: boolean;
  paging?: {
    /**
     * Whether to show the paging.
     */
    showPaging?: boolean;
    /**
     * Manual paging is server-side.
     */
    manualPagination?: boolean;
    /**
     * The current page number.
     */
    pageIndex?: number;
    /**
     * The number of rows.
     */
    pageSize?: number;
    /**
     * If `manualPagination=true` then this value is used to determine the amount of pages available.
     */
    pageCount?: number;
  };
}

/**
 * GridTable component provides a way to list and filter data in a grid table.
 * @param param1 GridTable properties.
 * @returns GridTable component.
 */
export const GridTable = <T extends object>({
  columns,
  data,
  onRowClick,
  onPageChange = () => {},
  showFooter = false,
  paging,
}: IGridTableProps<T>) => {
  const {
    showPaging = true,
    manualPagination = false,
    pageIndex: initialPage = 0,
    pageSize: initialPageSize = 10,
    pageCount: initialPageCount = -1,
  } = paging || {};
  const {
    getTableProps,
    getTableBodyProps,
    headerGroups,
    page,
    prepareRow,
    footerGroups,
    canPreviousPage,
    canNextPage,
    pageOptions,
    pageCount,
    gotoPage,
    nextPage,
    previousPage,
    setPageSize,
    state: { pageIndex, pageSize },
  } = useTable(
    {
      columns,
      data,
      manualPagination,
      pageCount: initialPageCount,
      initialState: {
        pageIndex: initialPage,
        pageSize: initialPageSize,
      },
    },
    usePagination,
  );
  const pager = {
    canPreviousPage,
    canNextPage,
    pageOptions,
    pageCount,
    gotoPage,
    nextPage,
    previousPage,
    setPageSize,
    pageIndex,
    pageSize,
  };

  React.useEffect(() => {
    onPageChange(pageIndex, pageSize);
  }, [onPageChange, pageIndex, pageSize]);

  return (
    <styled.GridTable {...getTableProps()}>
      <div role="rowheader">
        {headerGroups.map((headerGroup) => (
          <div role="rowgroup" {...headerGroup.getHeaderGroupProps()}>
            {headerGroup.headers.map((column) => (
              <div {...column.getHeaderProps()}>{column.render('Header')}</div>
            ))}
          </div>
        ))}
      </div>
      <div {...getTableBodyProps()}>
        {page.map((row) => {
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
      {showFooter && (
        <div>
          {footerGroups.map((footerGroup) => (
            <div role="row" {...footerGroup.getFooterGroupProps()}>
              {footerGroup.headers.map((column) => (
                <div role="cell" {...column.getFooterProps()}>
                  {column.render('Footer')}
                </div>
              ))}
            </div>
          ))}
        </div>
      )}
      {showPaging && <Pager {...pager} />}
    </styled.GridTable>
  );
};
