import React from 'react';
import { FaBackwardFast, FaCaretLeft, FaCaretRight, FaForwardFast } from 'react-icons/fa6';

import { Text } from './../form/text';

interface IGridPagerProps {
  pageIndex: number;
  itemsPerPage: number;
  totalItems?: number;
  isOneBasedIndexing?: boolean;
  onNavigatePage?: (page: number) => void;
  onQuantityChange?: (quantity: number) => void;
}

/**
 * Provides a grid that supports sorting and paging.
 * @param param0 Component properties.
 * @returns Component.
 */
export const GridPager = ({
  pageIndex,
  itemsPerPage,
  totalItems,
  isOneBasedIndexing = false,
  onNavigatePage,
  onQuantityChange,
}: IGridPagerProps) => {
  const [quantity, setQuantity] = React.useState(itemsPerPage);

  const numberOfPages = itemsPerPage > 0 && totalItems ? Math.ceil(totalItems / itemsPerPage) : 1;
  const infinitePages = !totalItems;
  const page = isOneBasedIndexing ? pageIndex : pageIndex + 1;

  React.useEffect(() => {
    setQuantity(itemsPerPage);
  }, [itemsPerPage]);

  return (
    <div className="grid-pager">
      <div>
        Page {page} of {infinitePages ? '*' : numberOfPages}
      </div>
      <div>
        <FaBackwardFast
          title="first"
          className={page === 1 ? 'disabled' : ''}
          onClick={() => onNavigatePage?.(isOneBasedIndexing ? 1 : 0)}
        />
      </div>
      <div>
        <FaCaretLeft
          title="previous"
          className={page === 1 ? 'disabled' : ''}
          onClick={() => {
            const prevPage = pageIndex - 1;
            if (prevPage >= 0) {
              onNavigatePage?.(prevPage);
            }
          }}
        />
      </div>
      <div>{page}</div>
      <div>
        <FaCaretRight
          title="next"
          className={page === numberOfPages && !infinitePages ? 'disabled' : ''}
          onClick={() => {
            const nextPage = pageIndex + 1;
            if (
              (isOneBasedIndexing ? nextPage < numberOfPages + 1 : nextPage < numberOfPages) ||
              infinitePages
            ) {
              onNavigatePage?.(nextPage);
            }
          }}
        />
      </div>
      <div>
        <FaForwardFast
          title="last"
          className={page === numberOfPages ? 'disabled' : ''}
          onClick={() => onNavigatePage?.(isOneBasedIndexing ? numberOfPages : numberOfPages - 1)}
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
  );
};
