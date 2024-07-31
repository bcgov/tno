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

export interface IGridColumnProps {
  key?: string | number;
  column?: React.ReactNode;
  isSelected?: boolean;
  isFocused?: boolean;
}

interface IGridProps<T> {
  className?: string;
  items: T[];
  pageIndex?: number;
  itemsPerPage?: number;
  totalItems?: number;
  isOneBasedIndexing?: boolean;
  renderHeader?: () => (React.ReactNode | IGridHeaderColumnProps)[];
  showPaging?: boolean;
  renderColumns: (row: T, index?: number) => (React.ReactNode | IGridColumnProps)[];
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
  isOneBasedIndexing = false,
  renderHeader,
  renderColumns,
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
      <div>
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
        </div>
        <div className="grid-table">
          {items?.map((item, rowIndex) =>
            renderColumns(item, rowIndex).map((column, columnIndex) => {
              const _column = column as IGridColumnProps;
              const _node = column as React.ReactNode;
              const key = _column.key ?? `${rowIndex}-${columnIndex}`;
              const body = _column.column ?? _node;
              const isActive =
                (typeof _column.isSelected === 'boolean' && _column.isSelected) ||
                (typeof _column.isFocused === 'boolean' && _column.isFocused)
                  ? 'active'
                  : '';
              return (
                <div key={key} className={`grid-column ${isActive}`}>
                  {body}
                </div>
              );
            }),
          )}
        </div>
      </div>
      {showPaging && (
        <GridPager
          pageIndex={isOneBasedIndexing ? pageIndex + 1 : pageIndex}
          itemsPerPage={itemsPerPage}
          totalItems={totalItems}
          onNavigatePage={onNavigatePage}
          onQuantityChange={onQuantityChange}
          isOneBasedIndexing={isOneBasedIndexing}
        />
      )}
    </styled.Grid>
  );
};
