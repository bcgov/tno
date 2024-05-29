import React from 'react';

import { SortDirection } from './constants';
import { GridPager } from './GridPager';
import { SortAction } from './SortAction';
import * as styled from './styled';

export interface IGridHeaderColumnProps {
  name?: string;
  label?: React.ReactNode;
  sortable?: boolean;
  size?: string;
}

interface IGridProps<T> {
  className?: string;
  items: T[];
  pageIndex?: number;
  itemsPerPage?: number;
  totalItems?: number;
  renderHeader?: () => (React.ReactNode | IGridHeaderColumnProps)[];
  showPaging?: boolean;
  renderRow: (row: T, index?: number) => React.ReactNode[];
  onNavigatePage?: (page: number) => void;
  onQuantityChange?: (quantity: number) => void;
  onSortChange?: (column: IGridHeaderColumnProps, direction: SortDirection) => void;
}

/**
 * Provides a grid that supports sorting and paging.
 * @param param0 Component properties.
 * @returns Component.
 */
export const Grid = <T,>({
  items,
  pageIndex = 0,
  itemsPerPage = 50,
  totalItems,
  showPaging,
  className,
  renderHeader,
  renderRow,
  onNavigatePage,
  onQuantityChange,
  onSortChange,
}: IGridProps<T>) => {
  const headers = renderHeader?.() ?? [];
  const columns = headers.map((col) => {
    const _column = col as IGridHeaderColumnProps;
    return _column.size ? _column : undefined;
  });

  const [sorting, setSorting] = React.useState<SortDirection[]>(
    headers.map(() => SortDirection.None),
  );

  return (
    <styled.Grid className={`grid${className ? ` ${className}` : ''}`} columns={columns}>
      <div className="grid-table">
        {headers.map((column, columnIndex) => {
          const _column = column as IGridHeaderColumnProps;
          const _node = column as React.ReactNode;
          const isColumn = column?.hasOwnProperty('label');

          return (
            <div key={columnIndex} className="grid-header">
              {isColumn ? _column.label : _node}
              {isColumn && _column.sortable && _column.name && (
                <SortAction
                  direction={sorting.length > columnIndex ? sorting[columnIndex] : undefined}
                  onChange={(direction) => {
                    setSorting((values) => {
                      return values.map((value, index) =>
                        index === columnIndex ? direction : SortDirection.None,
                      );
                    });
                    onSortChange?.(_column, direction);
                  }}
                />
              )}
            </div>
          );
        })}
        {items?.map((item, rowIndex) =>
          renderRow(item, rowIndex).map((column, columnIndex) => (
            <div key={`${rowIndex}-${columnIndex}`} className="grid-column">
              {column}
            </div>
          )),
        )}
      </div>
      {showPaging && (
        <GridPager
          pageIndex={pageIndex}
          itemsPerPage={itemsPerPage}
          totalItems={totalItems}
          onNavigatePage={onNavigatePage}
          onQuantityChange={onQuantityChange}
        />
      )}
    </styled.Grid>
  );
};
