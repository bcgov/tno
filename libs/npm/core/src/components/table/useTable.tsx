import React from 'react';

import {
  ITableHook,
  ITableInternal,
  ITableInternalRow,
  ITablePage,
  ITableSort,
  TableInternal,
} from '.';

/**
 * This hook generates a table object and manages state mutations.
 * Only the following properties will mutate externally; columns, data, options, paging, sorting.
 * @param param0 Hook properties
 * @returns A TableInternal object instance.
 */
export const useTable = <T extends object>({
  rowId,
  columns,
  data,
  options: initOptions,
  paging: initPaging,
  sorting: initSorting,
  filter: initFilter,
  grouping,
}: ITableHook<T>): ITableInternal<T> => {
  // Capture original values to determine if it should update the table
  const [paging, setPaging] = React.useState(initPaging);
  const [options, setOptions] = React.useState(initOptions);
  const [sorting, setSorting] = React.useState(initSorting ?? {});
  const [filter, setFilter] = React.useState(initFilter ?? {});

  // Initialize the original state of the table.
  const [table, setTable] = React.useState<ITableInternal<T>>(
    TableInternal.create(rowId, columns, data, options, paging, sorting, filter, grouping),
  );

  // When parent option properties changes we need to update state.
  React.useEffect(() => {
    if (
      initOptions.showFooter !== options.showFooter ||
      initOptions.showHeader !== options.showHeader ||
      initOptions.activeRowId !== options.activeRowId ||
      initOptions.selectedRowIds !== options.selectedRowIds ||
      initOptions.onRowClick !== options.onRowClick ||
      initOptions.onColumnClick !== options.onColumnClick ||
      initOptions.onCellClick !== options.onCellClick ||
      initOptions.onSelectedChanged !== options.onSelectedChanged
    ) {
      setOptions(initOptions);
    }
  }, [initOptions, options]);

  // When parent paging properties changes we need to update state.
  React.useEffect(() => {
    if (
      initPaging.pageIndex !== paging.pageIndex ||
      initPaging.pageSize !== paging.pageSize ||
      initPaging.showPaging !== paging.showPaging ||
      initPaging.onPageChange !== paging.onPageChange
    ) {
      setPaging(initPaging);
    }
  }, [initPaging, paging]);

  // When parent sorting properties changes we need to update state.
  React.useEffect(() => {
    if (
      initSorting?.showSort !== sorting.showSort ||
      initSorting?.sortOrder !== sorting.sortOrder ||
      initSorting?.onSortChange !== sorting.onSortChange
    ) {
      setSorting(initSorting ?? {});
    }
  }, [initSorting, sorting]);

  // When parent filter properties changes we need to update state.
  React.useEffect(() => {
    if (initFilter?.search !== filter.search) {
      setFilter(initFilter ?? {});
    }
  }, [initFilter, filter]);

  const compPage = React.useMemo(
    () => ({
      ...paging,
      pageIndex: initPaging.pageIndex !== paging.pageIndex ? initPaging.pageIndex : table.pageIndex,
      pageSize: initPaging.pageSize !== paging.pageSize ? initPaging.pageSize : table.pageSize,
      pageCount: initPaging.pageCount !== paging.pageCount ? initPaging.pageCount : table.pageCount,
    }),
    [
      paging,
      initPaging.pageIndex,
      initPaging.pageSize,
      initPaging.pageCount,
      table.pageIndex,
      table.pageSize,
      table.pageCount,
    ],
  );

  const compSorting = React.useMemo(
    () => ({
      ...sorting,
      showSort:
        initSorting?.showSort !== sorting?.showSort ? initSorting?.showSort : table.showSort,
      sortOrder:
        initSorting?.sortOrder !== sorting?.sortOrder ? initSorting?.sortOrder : table.sortOrder,
    }),
    [initSorting?.showSort, initSorting?.sortOrder, sorting, table.showSort, table.sortOrder],
  );

  const compFilter = React.useMemo(
    () => ({
      ...filter,
      search: initFilter?.search !== filter?.search ? initFilter?.search : table.search,
    }),
    [initFilter?.search, filter, table.search],
  );

  // When parent props change we need to recreate the table.
  React.useEffect(() => {
    setTable((table) => {
      const onRowClick = (
        row: ITableInternalRow<T>,
        event: React.MouseEvent<Element, MouseEvent>,
      ) => {
        row.table.setActiveRow(row);
        setTable({ ...row.table });
        options.onRowClick?.(row, event);
      };

      const onSelectedChanged = (
        row: ITableInternalRow<T>,
        event: React.ChangeEvent<HTMLInputElement>,
      ) => {
        row.table.toggleSelectedRow(row);
        setTable({ ...row.table });
        options.onSelectedChanged?.(row, event);
      };

      const onPageChange = (page: ITablePage, table: ITableInternal<T>) => {
        setTable({ ...table });
        paging.onPageChange?.(page, table);
      };

      const onSortChange = (sort: ITableSort<T>[], table: ITableInternal<T>) => {
        setTable({ ...table });
        sorting.onSortChange?.(sort, table);
      };

      const onFilterChange = (search: string, table: ITableInternal<T>) => {
        setTable({ ...table });
        filter.onFilterChange?.(search, table);
      };

      const result = TableInternal.create(
        rowId,
        columns,
        data,
        {
          ...options,
          onRowClick,
          onSelectedChanged,
        },
        {
          ...compPage,
          onPageChange,
        },
        {
          ...compSorting,
          onSortChange,
        },
        {
          ...compFilter,
          onFilterChange,
        },
        grouping,
        table.rows,
      );
      return result;
    });
    // This is to handle parent property mutations.
    // Ignore grouping changes.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [rowId, columns, compFilter, compPage, compSorting, data, options, paging, sorting, filter]);

  return table;
};
