import { Column, Row, SortingRule } from 'react-table';

import { GridTable, IPage } from '.';

export interface IPagedTableProps<CT extends object = {}> {
  pageIndex?: number;
  pageSize?: number;
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
  onChangePage: (pageIndex: number, pageSize?: number) => void;
  /**
   * The sort has changed.
   */
  onChangeSort?: (sortBy: Array<SortingRule<CT>>) => void;
  /**
   * Initial sorting rules.
   */
  sortBy?: Array<SortingRule<CT>>;
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
  sortBy,
}: IPagedTableProps<CT>) => {
  return (
    <GridTable
      columns={columns}
      data={page.items}
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
      onRowClick={onRowClick}
      onChangePage={onChangePage}
      onChangeSort={onChangeSort}
    ></GridTable>
  );
};
