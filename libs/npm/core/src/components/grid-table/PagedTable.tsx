import { Column, Row, SortingRule } from 'react-table';

import { GridTable, IPage } from '.';

export interface IPagedTableProps<CT extends object = {}> {
  /**
   * An array of column definitions.
   */
  columns: Column<CT>[];
  /**
   * A page of data.
   */
  page: IPage<CT>;
  /**
   * Handle row click event.
   */
  onRowClick?: (row: Row<CT>) => void;
  /**
   * Event fires when pageIndex or pageSize changes.
   */
  onChangePage?: (pageIndex: number, pageSize?: number) => void;
  /**
   * The sort has changed.
   */
  onChangeSort?: (sortBy: Array<SortingRule<CT>>) => void;
  /**
   * Initial sorting rules.
   */
  sortBy?: Array<SortingRule<CT>>;
  /**
   * Flag to indicate whether table is loading data or not.
   */
  isLoading?: boolean;
}

/**
 * A PagedTable component creates a table with server-side paging and sorting.
 * @param param0 Component properties.
 * @returns A component that displays a page of data.
 */
export const PagedTable = <CT extends object>({
  page,
  columns,
  onRowClick,
  onChangePage,
  onChangeSort,
  isLoading,
  sortBy,
}: IPagedTableProps<CT>) => {
  return (
    <GridTable
      columns={columns}
      data={page.items}
      isLoading={isLoading}
      paging={{
        manualPagination: true,
        pageIndex: page.pageIndex,
        pageSize: page.pageSize,
        pageCount: page.pageCount,
      }}
      sorting={{
        manualSortBy: true,
        sortBy: sortBy,
      }}
      filters={{
        manualFilters: true,
      }}
      onRowClick={onRowClick}
      onChangePage={onChangePage}
      onChangeSort={onChangeSort}
    ></GridTable>
  );
};
