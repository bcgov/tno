import React from 'react';
import { FaBackwardFast, FaCaretLeft, FaCaretRight, FaForwardFast } from 'react-icons/fa6';
import { IPaged, Text } from 'tno-core';

import { SortAction, SortDirection } from './SortAction';
import * as styled from './styled';

export interface IGridHeaderColumnProps {
  name?: string;
  label?: React.ReactNode;
  sortable?: boolean;
  size?: string;
}

interface IGridProps<T> {
  className?: string;
  data: IPaged<T>;
  renderHeader?: () => (React.ReactNode | IGridHeaderColumnProps)[];
  showPaging?: boolean;
  renderRow: (row: T, index?: number) => React.ReactNode[];
  onNavigatePage?: (page: number) => void;
  onQuantityChange?: (quantity: number) => void;
  onSortChange?: (column: IGridHeaderColumnProps, direction: SortDirection) => void;
}

export const Grid = <T,>({
  data,
  showPaging,
  className,
  renderHeader,
  renderRow,
  onNavigatePage,
  onQuantityChange,
  onSortChange,
}: IGridProps<T>) => {
  const numberOfPages = Math.ceil(data.total / data.quantity);
  const headers = renderHeader?.() ?? [];
  const columns = headers.map((col) => {
    const _column = col as IGridHeaderColumnProps;
    return _column.size ? _column : undefined;
  });

  const [quantity, setQuantity] = React.useState(data.quantity);
  const [sorting, setSorting] = React.useState<SortDirection[]>(
    headers.map(() => SortDirection.None),
  );

  React.useEffect(() => {
    setQuantity(data.quantity);
  }, [data.quantity]);

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
        {data.items.map((item, rowIndex) =>
          renderRow(item, rowIndex).map((column, columnIndex) => (
            <div key={`${rowIndex}-${columnIndex}`} className="grid-column">
              {column}
            </div>
          )),
        )}
      </div>
      {showPaging && (
        <div className="grid-pager">
          <div>
            Page {data.page} of {numberOfPages}
          </div>
          <div>
            <FaBackwardFast
              title="first"
              className={data.page === 1 ? 'disabled' : ''}
              onClick={() => data.page > 1 && onNavigatePage?.(1)}
            />
          </div>
          <div>
            <FaCaretLeft
              title="previous"
              className={data.page === 1 ? 'disabled' : ''}
              onClick={() => data.page > 1 && onNavigatePage?.(data.page - 1)}
            />
          </div>
          <div>{data.page}</div>
          <div>
            <FaCaretRight
              title="next"
              className={data.page === numberOfPages ? 'disabled' : ''}
              onClick={() => data.page < numberOfPages && onNavigatePage?.(data.page + 1)}
            />
          </div>
          <div>
            <FaForwardFast
              title="last"
              className={data.page === numberOfPages ? 'disabled' : ''}
              onClick={() => data.page < numberOfPages && onNavigatePage?.(numberOfPages)}
            />
          </div>
          <div>
            <Text
              name="quantity"
              value={quantity}
              type="number"
              width="8ch"
              onChange={(e) => {
                const value = parseInt(e.currentTarget.value);
                setQuantity(value);
              }}
              onBlur={() => {
                if (quantity > 0) onQuantityChange?.(quantity);
              }}
              onKeyDown={(e) => {
                if (e.code === 'Enter' && quantity > 0) onQuantityChange?.(quantity);
              }}
            />
          </div>
        </div>
      )}
    </styled.Grid>
  );
};
