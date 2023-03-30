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
import { IPageSizeOptions, Pager, SortIndicator } from '.';
import * as styled from './styled';

export enum PagingType {
  /** Show the pager */
  Show = 'show',
  /** Hide the pager */
  Hide = 'hide',
  /** Hide the pager and provide infinite scroll */
  InfiniteScroll = 'infinite',
}

/**
 * GridTable properties.
 */
export interface IGridTableProps<T extends object = Record<string, unknown>> {
  /** Class name */
  className?: string;
  /** An array of column definitions. */
  columns: Column<T>[];
  /** An array of row data. */
  data: T[];
  /** Handle row click event. */
  onRowClick?: (row: Row<T>) => void;
  /** The page has changed. */
  onChangePage?: (pageIndex: number, pageSize: number, instance: TableInstance<T>) => void;
  /** The sort has changed. */
  onChangeSort?: (sortBy: Array<SortingRule<T>>, instance: TableInstance<T>) => void;
  /** Header components. */
  header?: React.ComponentType<any>;
  /** Whether to show the footer. */
  showFooter?: boolean;
  /** Paging settings. */
  paging?: {
    /** The type of paging */
    type?: PagingType | 'show' | 'hide' | 'infinite';
    /** Manual paging is server-side. */
    manualPagination?: boolean;
    /** The current page number. */
    pageIndex?: number;
    /** The number of rows. */
    pageSize?: number;
    /** If `manualPagination=true` then this value is used to determine the amount of pages available. */
    pageCount?: number;
    /** Page sizing options. */
    pageSizeOptions?: IPageSizeOptions;
  };
  sorting?: {
    /** Whether to manually manage sorting. */
    manualSortBy?: boolean;
    /** Initial sorting rules. */
    sortBy?: Array<SortingRule<T>>;
  };
  filters?: {
    /** Whether to manually manage filtering. */
    manualFilters?: boolean;
  };
  /** Flag to indicate whether table is loading data or not. */
  isLoading?: boolean;
  /** Provide an array of columns to hide from the table */
  hiddenColumns?: IdType<T>[];
  /** How to get the row Id.  By default it uses the row index position. */
  getRowId?: (originalRow: T, relativeIndex: number, parent?: Row<T> | undefined) => string;
  /** Whether to enable multi select.  Defaults to false. */
  isMulti?: boolean;
  /** Whether selected rows should be automatically reset when data loads.  Defaults to true. */
  autoResetSelectedRows?: boolean;
  /** Whether to maintain selected rows when data is refreshed.  Defaults to true. */
  maintainSelectedRows?: boolean;
  /** The initial selected rows. */
  selectedRowIds?: Record<IdType<T>, boolean>;
  /** Event fires when active row changes. */
  onActiveRowChange?: (row?: Row<T>) => void;
  /** Event fires when selected rows changes. */
  onSelectedRowsChange?: (selectedRows: Row<T>[], instance: TableInstance<T>) => void;
  /** Whether to add to infinite items - helps the system to determine whether to clear infinite list if filter is applied. */
  setAddToInfiniteItems?: (add: boolean) => void;
  /** Whether a row can be selected. */
  enableRowSelect?: boolean;
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
  isMulti,
  autoResetSelectedRows = true,
  maintainSelectedRows = true,
  selectedRowIds: initSelectedRowIds,
  onSelectedRowsChange,
  onActiveRowChange,
  getRowId,
  setAddToInfiniteItems,
  enableRowSelect = true,
}: IGridTableProps<T>) => {
  const {
    type = PagingType.Show,
    manualPagination = false,
    pageIndex: initialPageIndex = 0,
    pageSize: initialPageSize = 20,
    pageCount: initialPageCount = -1,
    pageSizeOptions,
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
    pageSizeOptions,
  };

  const [activeRow, setActiveRow] = React.useState<Row<T>>();
  const [initialSelectedRowIds, setInitialSelectedRowIds] = React.useState(initSelectedRowIds);

  // for infinite scrolling
  const observer = React.useRef<IntersectionObserver>();
  // ref is only used in element if infiniteScroll is true
  const lastRowRef = React.useCallback(
    (node: HTMLDivElement) => {
      if (isLoading) return;
      if (observer.current) observer.current.disconnect();
      observer.current = new IntersectionObserver((entries) => {
        if (entries[0].isIntersecting && canNextPage) {
          setAddToInfiniteItems && setAddToInfiniteItems(true);
          nextPage();
        }
      });
      if (node) observer.current.observe(node);
    },
    [isLoading, canNextPage, setAddToInfiniteItems, nextPage],
  );

  const changeActiveRow = React.useCallback(
    (row: Row<T> | undefined) => {
      if (row?.id !== activeRow?.id) {
        setActiveRow(row);
        onActiveRowChange?.(row);
      }
    },
    [activeRow, onActiveRowChange],
  );

  React.useEffect(() => {
    if (!maintainSelectedRows) instance.toggleAllRowsSelected(false);
  }, [data, instance, maintainSelectedRows]);

  React.useEffect(() => {
    // This solves an issue when a page contains more items than the pageSize.
    if (!manualPagination && type !== PagingType.Show && data.length && pageSize < data.length) {
      instance.setPageSize(data.length);
    }
  }, [data, instance, manualPagination, pageSize, type]);

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
    const initRowIds = Object.keys(
      (initialSelectedRowIds ?? {}) as Record<string, boolean>,
    ).toString();
    const selectedRowIds = Object.keys((initSelectedRowIds ?? {}) as Record<string, boolean>);
    if (initRowIds !== selectedRowIds.toString()) {
      setInitialSelectedRowIds(initSelectedRowIds);
      // For each row toggle it if it needs to change based on the latest selected values.
      instance.rows.forEach((row) => {
        const selectId = selectedRowIds.find((id) => id === row.id);
        if (selectId) {
          row.toggleRowSelected(true);
          changeActiveRow(row);
        }
      });
    }
  }, [initSelectedRowIds, initialSelectedRowIds, instance, changeActiveRow]);

  React.useEffect(() => {
    if (!!selectedFlatRows) {
      onSelectedRowsChange?.(selectedFlatRows, instance);
      if (selectedFlatRows.length === 0) {
        changeActiveRow(undefined);
      }
    }
    // For some reason 'onSelectedRowsChange' results in spam...
  }, [selectedFlatRows, onSelectedRowsChange, instance, changeActiveRow]);

  const handleRowClick = (e: React.MouseEvent<HTMLDivElement, MouseEvent>, row: Row<T>) => {
    // Deselect all selected rows on a normal click.
    if (!isMulti || (!e.ctrlKey && !row.isSelected)) instance.toggleAllRowsSelected(false);
    if (!row.isSelected) {
      // Make the row selected and active.
      row.toggleRowSelected(true);
      changeActiveRow(row);
    } else if (activeRow?.id === row.id) {
      // Only change the active row if ctrl is pressed.
      if (e.ctrlKey && selectedFlatRows.length > 1) {
        const index = selectedFlatRows[0].index === row.index ? 1 : 0;
        row.toggleRowSelected(false);
        changeActiveRow(selectedFlatRows[index]);
      }
    } else if (!e.ctrlKey) {
      // Selecting an active row when multiple are selected will just change the active row.
      changeActiveRow(row);
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
    <styled.GridTable
      className={`table${className ? ` ${className}` : ''}`}
      enableRowSelect={enableRowSelect}
      {...getTableProps()}
    >
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
          if (page.length === index + 1 && type === PagingType.InfiniteScroll) {
            return (
              <div
                {...row.getRowProps()}
                onClick={(e) => (enableRowSelect ? handleRowClick(e, row) : undefined)}
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
                onClick={(e) => (enableRowSelect ? handleRowClick(e, row) : undefined)}
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
      {type === PagingType.Show && <Pager {...pager} />}
    </styled.GridTable>
  );
};
