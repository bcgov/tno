import React from 'react';
import {
  Column,
  IdType,
  Row,
  SortingRule,
  useFilters,
  useFlexLayout,
  useGlobalFilter,
  usePagination,
  useSortBy,
  useTable,
} from 'react-table';

import { Row as FlexRow } from '../flex';
import { Spinner } from '../spinners';
import { Pager, SortIndicator } from '.';
import * as styled from './styled';

/**
 * GridTable properties.
 */
export interface IGridTableProps<CT extends object = Record<string, unknown>> {
  /** Class name */
  className?: string;
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
  onChangePage?: (pageIndex: number, pageSize?: number) => void;
  /**
   * The sort has changed.
   */
  onChangeSort?: (sortBy: Array<SortingRule<CT>>) => void;
  /**
   * Header components.
   */
  header?: React.ComponentType<any>;
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
  sorting?: {
    /**
     * Whether to manually manage sorting.
     */
    manualSortBy?: boolean;
    /**
     * Initial sorting rules.
     */
    sortBy?: Array<SortingRule<CT>>;
  };
  filters?: {
    /**
     * Whether to manually manage filtering.
     */
    manualFilters?: boolean;
  };
  /**
   * Flag to indicate whether table is loading data or not.
   */
  isLoading?: boolean;
  /** Pass the table the active row id to highlight it */
  activeId?: number | string;
  /** The assessor property name to identify the active row. */
  activeAssessor?: string;
  /**
   * Whether to include manual page sizing on the pager
   */
  manualPageSize?: boolean;
  /**
   * Provide an array of columns to hide from the table
   */
  hiddenColumns?: IdType<CT>[];
}

/**
 * GridTable component provides a way to list and filter data in a grid table.
 * @param param1 GridTable properties.
 * @returns GridTable component.
 */
export const GridTable = <T extends object>({
  className,
  columns,
  data,
  onRowClick,
  onChangePage = () => {},
  onChangeSort = () => {},
  header: Header,
  showFooter = false,
  paging,
  sorting,
  isLoading,
  hiddenColumns = [],
  filters,
  activeId,
  activeAssessor = 'id',
  manualPageSize,
}: IGridTableProps<T>) => {
  const {
    showPaging = true,
    manualPagination = false,
    pageIndex: initialPageIndex = 0,
    pageSize: initialPageSize = 10,
    pageCount: initialPageCount = -1,
  } = paging || {};
  const { manualFilters = false } = filters || {};
  const { manualSortBy = false, sortBy: initialSortBy = [] } = sorting || {};
  const instance = useTable(
    {
      columns,
      data,
      manualPagination: manualPagination,
      manualSortBy: manualSortBy,
      manualFilters: manualFilters,
      pageCount: initialPageCount,
      initialState: {
        pageIndex: initialPageIndex,
        pageSize: initialPageSize,
        sortBy: initialSortBy,
        hiddenColumns: hiddenColumns,
      },
    },
    useFilters,
    useGlobalFilter,
    useSortBy,
    usePagination,
    useFlexLayout,
  );
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
    setHiddenColumns,
    state: { pageIndex, pageSize, sortBy },
  } = instance;
  const [currentPage, setCurrentPage] = React.useState({ pageIndex, pageSize });
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

  // The user / system disables a column
  React.useEffect(() => {
    setHiddenColumns(hiddenColumns);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [JSON.stringify(hiddenColumns), setHiddenColumns]);

  // The user has manually changed the pageIndex.
  React.useEffect(() => {
    gotoPage(initialPageIndex);
  }, [gotoPage, initialPageIndex]);

  // Pagination has detected a change to the page.
  React.useEffect(() => {
    // Only notify parent if the change came internally.
    if (currentPage.pageIndex !== pageIndex || currentPage.pageSize !== pageSize) {
      onChangePage(pageIndex, pageSize);
      setCurrentPage({ pageIndex, pageSize });
    }
  }, [onChangePage, pageIndex, pageSize, currentPage]);

  React.useEffect(() => {
    if (sortBy.length !== initialSortBy.length || !sortBy.every((v, i) => v === initialSortBy[i]))
      onChangeSort(sortBy);
  }, [initialSortBy, onChangeSort, sortBy]);

  return (
    <styled.GridTable className={`table${className ? ` ${className}` : ''}`} {...getTableProps()}>
      {Header && <Header {...instance} />}
      <div role="rowheader">
        {headerGroups.map((headerGroup) => (
          <div className="rh" {...headerGroup.getHeaderGroupProps()}>
            {headerGroup.headers.map((column) => (
              <div {...column.getHeaderProps(column.getSortByToggleProps())}>
                {column.render('Header') as unknown as React.ReactNode}
                <SortIndicator column={column} />
              </div>
            ))}
          </div>
        ))}
      </div>
      {isLoading ? (
        <FlexRow justifyContent="center">
          <Spinner />
        </FlexRow>
      ) : (
        <div {...getTableBodyProps()}>
          {page.map((row) => {
            prepareRow(row);
            return (
              <div
                className={
                  !!activeId && activeId === (row.original as any)[activeAssessor] ? 'active' : ''
                }
                {...row.getRowProps()}
                onClick={() => onRowClick && onRowClick(row)}
              >
                {row.cells.map((cell) => {
                  return (
                    <div {...cell.getCellProps()}>
                      {cell.render('Cell') as unknown as React.ReactNode}
                    </div>
                  );
                })}
              </div>
            );
          })}
        </div>
      )}

      {showFooter && (
        <div>
          {footerGroups.map((footerGroup) => (
            <div role="row" {...footerGroup.getFooterGroupProps()}>
              {footerGroup.headers.map((column) => (
                <div role="cell" {...column.getFooterProps()}>
                  {column.render('Footer') as unknown as React.ReactNode}
                </div>
              ))}
            </div>
          ))}
        </div>
      )}
      {showPaging && <Pager manualPageSize={manualPageSize} {...pager} />}
    </styled.GridTable>
  );
};
