import React from 'react';
import { Column, Row } from 'react-table';

import { GridTable, IPage } from '.';

export interface IPagedTableProps<CT extends object = {}> {
  /**
   * An array of column definitions.
   */
  columns: Column<CT>[];
  /**
   * Handle row click event.
   */
  onRowClick?: (row: Row<CT>) => void;
  /**
   * Method to fetch data.
   */
  fetchData: (pageIndex: number, pageSize?: number) => Promise<IPage<CT>>;
}

export const PagedTable = <CT extends object>({
  columns,
  onRowClick,
  fetchData,
}: IPagedTableProps<CT>) => {
  const [pageIndex, setPageIndex] = React.useState(0);
  const [pageSize, setPageSize] = React.useState(10);
  const [pageCount, setPageCount] = React.useState(-1);
  const [data, setData] = React.useState<CT[]>([]);

  React.useEffect(() => {
    const fetch = async (pageIndex: number, pageSize?: number) => {
      return await fetchData(pageIndex, pageSize);
    };
    fetch(pageIndex, pageSize)
      .then((page) => {
        setPageIndex(page.pageIndex);
        setPageSize(page.pageSize);
        setPageCount(page.pageCount);
        setData(page.items);
      })
      .catch((error) => {
        // TODO: Handle error.
      });
  }, [fetchData, pageIndex, pageSize]);

  const handlePageChange = React.useCallback((pageIndex: number, pageSize?: number) => {
    setPageIndex(pageIndex);
    setPageSize(pageSize ?? 10);
  }, []);

  return (
    <GridTable
      columns={columns}
      data={data}
      paging={{
        manualPagination: true,
        pageIndex: pageIndex,
        pageSize: pageSize,
        pageCount: pageCount,
      }}
      onRowClick={onRowClick}
      onPageChange={handlePageChange}
    ></GridTable>
  );
};
