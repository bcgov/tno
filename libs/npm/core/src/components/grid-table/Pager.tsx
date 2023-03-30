import React from 'react';
import { BiFirstPage, BiLastPage } from 'react-icons/bi';
import { useLocation, useSearchParams } from 'react-router-dom';

import { Button, ButtonVariant } from '../button';
import { Row } from '../flex';
import { Text } from '../form/text';
import { Show } from '../show';
import * as styled from './styled';

const QTY = 'pageSize';

/** Page size options */
export interface IPageSizeOptions {
  /** Whether to show the page quantity component. */
  show?: boolean;
  /** Whether to extract the quantity from the URL. */
  fromUrl?: boolean;
  /** Whether to extract the quantity from local storage. */
  fromLocalStorage?: boolean;
  /** The key name of the pageSize parameter. */
  key?: string;
}

/** Pager properties. */
export interface IPagerProps {
  /** Page index position. */
  pageIndex: number;
  /** Number of rows per page. */
  pageSize: number;
  /** Total number of pages. */
  pageCount: number;
  /** Number of pages to show in pager. */
  pageLimit?: number;
  /** Show paging group buttons. */
  showPageGroup?: boolean;
  /** Can go to previous page. */
  canPreviousPage: boolean;
  /** Can go to next page. */
  canNextPage: boolean;
  /** An array of zero-based index integers corresponding to available pages in the table. */
  pageOptions: number[];
  /** Page size options */
  pageSizeOptions?: IPageSizeOptions;
  /** Event when changing page number. */
  gotoPage: (updater: number | ((pageIndex: number) => number)) => void;
  /** Event when going to next page. */
  nextPage: () => void;
  /** Event when going to previous page. */
  previousPage: () => void;
  /** Event when page size changes. */
  setPageSize: (pageSize: number) => void;
}

/**
 * Provides a manually change the page.
 * @param param0 Component properties.
 * @returns Component.
 */
export const Pager: React.FC<IPagerProps> = ({
  pageIndex,
  pageSize,
  pageCount,
  pageLimit = 10,
  canPreviousPage,
  showPageGroup = false,
  canNextPage,
  pageOptions = [],
  pageSizeOptions = {},
  gotoPage,
  nextPage,
  previousPage,
  setPageSize,
}) => {
  const { search } = useLocation();
  let query = React.useMemo(() => new URLSearchParams(search), [search]);
  let [searchParams, setSearchParams] = useSearchParams(query);

  const [quantity, setQuantity] = React.useState(pageSize);

  const {
    show: showPageSize = true,
    fromUrl = true,
    fromLocalStorage = false,
    key = QTY,
  } = pageSizeOptions;

  const qtyUrl = Number(searchParams.get(key));
  const qtyLS = Number(localStorage.getItem(key));

  React.useEffect(() => {
    if (fromUrl && fromLocalStorage) {
      if (!isNaN(qtyUrl) && !!qtyUrl && qtyUrl !== qtyLS) {
        if (fromLocalStorage && qtyUrl !== qtyLS) localStorage.setItem(key, qtyUrl.toString());
      } else if (!isNaN(qtyLS) && !!qtyLS) {
        setSearchParams({ ...searchParams, [key]: qtyLS.toString() });
      }
    }
    // Extract quantity from URL and update local storage, or extract from local storage and update the URL the first time loading.
  }, [fromLocalStorage, fromUrl, key, qtyLS, qtyUrl, searchParams, setSearchParams]);

  React.useEffect(() => {
    // The URL has changed, update the page number.
    if (fromUrl) {
      if (!isNaN(qtyUrl) && !!qtyUrl && qtyUrl !== pageSize) setPageSize(qtyUrl);
    } else if (fromLocalStorage) {
      if (!isNaN(qtyLS) && !!qtyLS && qtyLS !== pageSize) setPageSize(qtyLS);
    }
    // Change the page size if the URL or local storage have a different value.
  }, [fromUrl, fromLocalStorage, pageSize, setPageSize, qtyUrl, qtyLS]);

  React.useEffect(() => {
    if (quantity !== pageSize) setQuantity(pageSize);
    // Only update the quantity if pageSize changes.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [pageSize]);

  const rows = React.useCallback(() => {
    // TODO: Handle dynamic pageLimit.  Calculate center.
    let startIndex = 0;
    if (pageIndex <= 2) startIndex = 0;
    else if (pageIndex >= pageCount - 2)
      startIndex = pageCount - pageLimit < 0 ? 0 : pageCount - pageLimit;
    else startIndex = pageIndex - 2;

    const maxIndex =
      startIndex + pageLimit - 1 > pageCount - 1 ? pageCount - 1 : startIndex + pageLimit - 1;
    const endIndex = pageCount <= startIndex ? startIndex : maxIndex;

    const rows = [];
    if (pageOptions.length) {
      for (let i = startIndex; i <= endIndex; i++) {
        rows.push(
          <Button
            key={pageOptions[i]}
            variant={ButtonVariant.info}
            onClick={() => {
              gotoPage(i);
            }}
            disabled={pageIndex === pageOptions[i]}
            className={pageIndex === pageOptions[i] ? 'active' : ''}
          >
            {pageOptions[i] + 1}
          </Button>,
        );
      }
    }
    return rows;
  }, [pageIndex, pageCount, pageLimit, pageOptions, gotoPage]);

  const onChangePageSize = React.useCallback(
    (qty: number) => {
      if (!!qty && qty !== pageSize) {
        if (fromLocalStorage) localStorage.setItem(key, [key].toString());
        if (fromUrl) setSearchParams({ ...searchParams, [key]: qty.toString() });
        setPageSize(qty); // TODO: Determine if this causes a double update.
      }
    },
    [fromLocalStorage, fromUrl, key, pageSize, searchParams, setPageSize, setSearchParams],
  );

  return (
    <styled.Pager>
      <Row justifyContent="center" className="button-container">
        <Button
          name="firstPage"
          tooltip="First Page"
          variant={ButtonVariant.info}
          onClick={() => gotoPage(0)}
          disabled={!canPreviousPage}
        >
          <BiFirstPage />
        </Button>
        {showPageGroup && (
          <Button
            name="previousPageGroup"
            variant={ButtonVariant.info}
            onClick={() => previousPage()}
            disabled={!canPreviousPage}
          >
            ...
          </Button>
        )}
        {rows()}
        {showPageGroup && (
          <Button
            name="nextPageGroup"
            variant={ButtonVariant.info}
            onClick={() => nextPage()}
            disabled={!canNextPage}
          >
            ...
          </Button>
        )}
        <Button
          name="lastPage"
          tooltip="Last Page"
          variant={ButtonVariant.info}
          onClick={() => gotoPage(pageCount - 1)}
          disabled={!canNextPage}
        >
          <BiLastPage />
        </Button>
        <Show visible={!!showPageSize}>
          <Text
            className="page-size"
            tooltip="Choose page size"
            value={quantity}
            type="number"
            name="pageSize"
            onKeyUp={(e) => {
              if (e.key === 'Enter') onChangePageSize(Number(e.currentTarget.value));
            }}
            onBlur={(e) => {
              onChangePageSize(Number(e.currentTarget.value));
            }}
            onChange={(e) => {
              setQuantity(Number(e.target.value));
            }}
          />
        </Show>
      </Row>
    </styled.Pager>
  );
};
