import { GridTable, IGridTableProps, IPage } from '.';

export interface IPagedTableProps<CT extends object = Record<string, unknown>>
  extends Omit<IGridTableProps<CT>, 'data'> {
  /**
   * A page of data.
   */
  page: IPage<CT>;
}

/**
 * A PagedTable component creates a table with server-side paging and sorting.
 * @param param0 Component properties.
 * @returns A component that displays a page of data.
 */
export const PagedTable = <CT extends object = Record<string, unknown>>({
  page,
  sorting,
  ...rest
}: IPagedTableProps<CT>) => {
  return (
    <GridTable
      data={page.items}
      paging={{
        manualPagination: true,
        pageIndex: page.pageIndex,
        pageSize: page.pageSize,
        pageCount: page.pageCount,
      }}
      sorting={{
        manualSortBy: true,
        sortBy: sorting?.sortBy,
      }}
      filters={{
        manualFilters: true,
      }}
      {...rest}
    ></GridTable>
  );
};
