import React from 'react';

import { GridTable, IGridTableProps, IPage, IPageSizeOptions, PagingType } from '.';

export interface IPagedTableProps<CT extends object = Record<string, unknown>>
  extends Omit<IGridTableProps<CT>, 'data'> {
  /** A page of data. */
  page: IPage<CT>;
  /** Page size options. */
  pageSizeOptions?: IPageSizeOptions;
}

/**
 * A PagedTable component creates a table with server-side paging and sorting.
 * @param param0 Component properties.
 * @returns A component that displays a page of data.
 */
export const PagedTable = <CT extends object = Record<string, unknown>>({
  page,
  pageSizeOptions,
  sorting,
  ...rest
}: IPagedTableProps<CT>) => {
  const [items, setItems] = React.useState<CT[]>(page.items);
  const [addToInfiniteItems, setAddToInfiniteItems] = React.useState<boolean>(false);

  React.useEffect(() => {
    if (addToInfiniteItems) {
      setItems((prevItems) => prevItems.concat(page.items));
      setAddToInfiniteItems(false);
    } else {
      // we don't want stale items when user changes sorting or filter from something other than scrolling
      setItems(page.items);
    }
  }, [addToInfiniteItems, page.items]);

  return (
    <GridTable
      data={rest.paging?.type === PagingType.InfiniteScroll ? items : page.items}
      paging={{
        manualPagination: true,
        pageIndex: page.pageIndex,
        pageSize: page.pageSize,
        pageCount: page.pageCount,
        pageSizeOptions,
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
