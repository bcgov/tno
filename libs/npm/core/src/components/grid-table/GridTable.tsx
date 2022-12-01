import React from 'react';
import {
  Column,
  IdType,
  Row,
  SortingRule,
  TableInstance,
  useFilters,
  useFlexLayout,
  useGlobalFilter,
  usePagination,
  useRowSelect,
  useSortBy,
  useTable,
} from 'react-table';

import { Loading, Show } from '..';
import { Pager, SortIndicator } from '.';
import * as styled from './styled';

/**
 * GridTable properties.
 */
export interface IGridTableProps<T extends object = Record<string, unknown>> {
  /** Class name */
  className?: string;
  /** Whether or not you wish to use the infinite scroll variant */
  infiniteScroll?: boolean;
  /**
   * An array of column definitions.
   */
  columns: Column<T>[];
  /**
   * An array of row data.
   */
  data: T[];
  /**
   * Handle row click event.
   */
  onRowClick?: (row: Row<T>) => void;
  /**
   * The page has changed.
   */
  onChangePage?: (pageIndex: number, pageSize: number, instance: TableInstance<T>) => void;
  /**
   * The sort has changed.
   */
  onChangeSort?: (sortBy: Array<SortingRule<T>>, instance: TableInstance<T>) => void;
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
    sortBy?: Array<SortingRule<T>>;
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
  /**
   * Whether to include manual page sizing on the pager
   */
  manualPageSize?: boolean;
  /**
   * Provide an array of columns to hide from the table
   */
  hiddenColumns?: IdType<T>[];
  /**
   * How to get the row Id.  By default it uses the row index position.
   */
  getRowId?: (originalRow: T, relativeIndex: number, parent?: Row<T> | undefined) => string;
  /**
   * Whether to enable multi select.  Defaults to false.
   */
  isMulti?: boolean;
  /**
   * Whether selected rows should be automatically reset when data loads.  Defaults to true.
   */
  autoResetSelectedRows?: boolean;
  /**
   * Whether to maintain selected rows when data is refreshed.  Defaults to true.
   */
  maintainSelectedRows?: boolean;
  /**
   * The initial selected rows.
   */
  selectedRowIds?: Record<IdType<T>, boolean>;
  /**
   * Event fires when selected rows changes.
   */
  onSelectedRowsChange?: (selectedRows: Row<T>[], instance: TableInstance<T>) => void;
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
  manualPageSize,
  isMulti,
  autoResetSelectedRows = true,
  maintainSelectedRows = true,
  selectedRowIds: initSelectedRowIds,
  onSelectedRowsChange,
  getRowId,
  infiniteScroll,
}: IGridTableProps<T>) => {
  const {
    showPaging = infiniteScroll ? false : true,
    manualPagination = false,
    pageIndex: initialPageIndex = 0,
    pageSize: initialPageSize = 20,
    pageCount: initialPageCount = -1,
  } = paging || {};
  const { manualFilters = false } = filters || {};
  const { manualSortBy = false, sortBy: initialSortBy = [] } = sorting || {};
  const instance = useTable(
    {
      columns,
      data,
      autoResetSelectedRows: autoResetSelectedRows,
      manualPagination: manualPagination,
      manualSortBy: manualSortBy,
      manualFilters: manualFilters,
      pageCount: initialPageCount,
      getRowId,
      initialState: {
        selectedRowIds: initSelectedRowIds ?? ({} as Record<IdType<T>, boolean>),
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
    useRowSelect,
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
    selectedFlatRows,
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

  const [activeRow, setActiveRow] = React.useState<Row<T>>();

  // for infinite scrolling
  const observer = React.useRef<IntersectionObserver>();
  // ref is only used in element if infiniteScroll is true
  const lastRowRef = React.useCallback(
    (node: HTMLDivElement) => {
      if (isLoading) return;
      if (observer.current) observer.current.disconnect();
      observer.current = new IntersectionObserver((entries) => {
        if (entries[0].isIntersecting && canNextPage) nextPage();
      });
      if (node) observer.current.observe(node);
    },
    [isLoading, canNextPage, setPageSize, currentPage.pageSize],
  );

  React.useEffect(() => {
    if (!maintainSelectedRows) instance.toggleAllRowsSelected(false);
  }, [data, instance, maintainSelectedRows]);

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
      onChangePage(pageIndex, pageSize, instance);
      setCurrentPage({ pageIndex, pageSize });
    }
  }, [onChangePage, pageIndex, pageSize, currentPage, instance]);

  React.useEffect(() => {
    if (
      sortBy.length !== initialSortBy.length ||
      !sortBy.every((v, i) => v.id === initialSortBy[i].id && v.desc === initialSortBy[i].desc)
    ) {
      onChangeSort(sortBy, instance);
    }
  }, [initialSortBy, instance, onChangeSort, sortBy]);

  React.useEffect(() => {
    if (!!selectedFlatRows) {
      onSelectedRowsChange?.(selectedFlatRows, instance);
      if (selectedFlatRows.length === 0) setActiveRow(undefined);
    }
    // For some reason 'onSelectedRowsChange' results in spam...
  }, [selectedFlatRows, onSelectedRowsChange, instance]);

  const handleRowClick = (e: React.MouseEvent<HTMLDivElement, MouseEvent>, row: Row<T>) => {
    // Deselect all selected rows on a normal click.
    if (!isMulti || (!e.ctrlKey && !row.isSelected)) instance.toggleAllRowsSelected(false);
    if (!row.isSelected) {
      // Make the row selected and active.
      row.toggleRowSelected(true);
      setActiveRow(row);
    } else if (activeRow?.id === row.id) {
      // Only change the active row if ctrl is pressed.
      if (e.ctrlKey && selectedFlatRows.length > 1) {
        const index = selectedFlatRows[0].index === row.index ? 1 : 0;
        row.toggleRowSelected(false);
        setActiveRow(selectedFlatRows[index]);
      }
    } else if (!e.ctrlKey) {
      // Selecting an active row when multiple are selected will just change the active row.
      setActiveRow(row);
    } else {
      // Remove selected item.
      row.toggleRowSelected(false);
    }
    onRowClick?.({ ...row, isSelected: !row.isSelected });
  };

  const onRowRenderClassName = (row: Row<T>) => {
    return `${row.isSelected ? 'selected' : ''}${activeRow?.id === row.id ? ' active' : ''}`;
  };

  return (
    <styled.GridTable className={`table${className ? ` ${className}` : ''}`} {...getTableProps()}>
      {Header && <Header {...instance} />}
      <div role="rowheader">
        {headerGroups.map((headerGroup) => (
          <div className="rh" {...headerGroup.getHeaderGroupProps()}>
            {headerGroup.headers.map((column) => (
              <div
                {...column.getHeaderProps(column.getSortByToggleProps())}
                className={`h-${column.id}`}
              >
                {column.render('Header') as unknown as React.ReactNode}
                <SortIndicator column={column} />
              </div>
            ))}
          </div>
        ))}
      </div>
      <div {...getTableBodyProps()}>
        {page.map((row, index) => {
          prepareRow(row);
          if (page.length === index + 1 && infiniteScroll) {
            return (
              <div
                {...row.getRowProps()}
                onClick={(e) => handleRowClick(e, row)}
                className={onRowRenderClassName(row)}
                ref={lastRowRef}
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
          } else
            return (
              <div
                {...row.getRowProps()}
                onClick={(e) => handleRowClick(e, row)}
                className={onRowRenderClassName(row)}
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
        <Show visible={isLoading}>
          <Loading />
        </Show>
      </div>

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
