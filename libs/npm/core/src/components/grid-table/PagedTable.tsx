import { GridTable, IGridTableProps, IPage } from '.';

export interface IPagedTableProps<CT extends object = Record<string, unknown>>
  extends Omit<IGridTableProps<CT>, 'data'> {
  /**
   * A page of data.
   */
  page: IPage<CT>;
  /** Pass the table the active row id to highlight it */
  activeId?: number;
}

/**
 * A PagedTable component creates a table with server-side paging and sorting.
 * @param param0 Component properties.
 * @returns A component that displays a page of data.
 */
export const PagedTable = <CT extends object = Record<string, unknown>>({
  page,
  columns,
  onRowClick,
  onChangePage,
  onChangeSort,
  isLoading,
  header,
  sorting,
  activeId,
}: IPagedTableProps<CT>) => {
  return (
    <GridTable
      columns={columns}
      data={page.items}
      isLoading={isLoading}
      activeId={activeId}
      header={header}
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
      onRowClick={onRowClick}
      onChangePage={onChangePage}
      onChangeSort={onChangeSort}
    ></GridTable>
  );
};
