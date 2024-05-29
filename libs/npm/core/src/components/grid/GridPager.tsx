import React from 'react';
import { FaBackwardFast, FaCaretLeft, FaCaretRight, FaForwardFast } from 'react-icons/fa6';

import { Text } from './../form/text';

interface IGridPagerProps {
  pageIndex: number;
  itemsPerPage: number;
  totalItems?: number;
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
  onNavigatePage,
  onQuantityChange,
}: IGridPagerProps) => {
  const [quantity, setQuantity] = React.useState(itemsPerPage);

  const numberOfPages = itemsPerPage > 0 && totalItems ? Math.ceil(totalItems / itemsPerPage) : 1;
  const infinitePages = !totalItems;
  const page = pageIndex + 1;

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
          onClick={() => page > 1 && onNavigatePage?.(1)}
        />
      </div>
      <div>
        <FaCaretLeft
          title="previous"
          className={page === 1 ? 'disabled' : ''}
          onClick={() => page > 1 && onNavigatePage?.(page - 1)}
        />
      </div>
      <div>{page}</div>
      <div>
        <FaCaretRight
          title="next"
          className={page === numberOfPages && !infinitePages ? 'disabled' : ''}
          onClick={() => (page < numberOfPages || infinitePages) && onNavigatePage?.(page + 1)}
        />
      </div>
      <div>
        <FaForwardFast
          title="last"
          className={page === numberOfPages ? 'disabled' : ''}
          onClick={() => (page < numberOfPages || infinitePages) && onNavigatePage?.(numberOfPages)}
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
