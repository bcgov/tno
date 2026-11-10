import { FormPage } from '@/editor/components/formpage';
import React from 'react';
import { FaRegClipboard } from 'react-icons/fa6';
import { useNavigate } from 'react-router-dom';
import { useFilters } from '@/editor/store/hooks/admin';
import { useAdminStore } from '@/editor/store/slices';
import {
  CellCheckbox,
  CellEllipsis,
  Col,
  Grid,
  IconButton,
  type IFilterModel,
  type IPaged,
  Link,
  Row,
  SortDirection,
} from 'tno-core';

import { FilterAISettings } from './FilterAISettings';
import { useGridBodyHeight } from './hooks';
import { ListFilter } from './ListFilter';
import * as styled from './styled';
import { handleCopyKeyWords } from './utils/handleCopyKeyWords';
import { truncateString } from './utils/truncateString';

/** The filters shown per page until the user picks another quantity. */
const DefaultPageSize = 50;

/**
 * Lists the filters a page at a time. The keyword search, sort and paging run on the server; the
 * table fills the viewable area and scrolls.
 */
export const FilterList: React.FC = () => {
  const navigate = useNavigate();
  const [, { findFilterPage }] = useFilters();
  const [{ filterFilter }] = useAdminStore();

  const [keyword, setKeyword] = React.useState(filterFilter);
  const [pageIndex, setPageIndex] = React.useState(0);
  const [pageSize, setPageSize] = React.useState(DefaultPageSize);
  const [sort, setSort] = React.useState<string[]>([]);
  const [page, setPage] = React.useState<IPaged<IFilterModel>>({
    page: 1,
    quantity: DefaultPageSize,
    total: 0,
    items: [],
  });
  const [isLoading, setIsLoading] = React.useState(false);
  const gridRef = React.useRef<HTMLDivElement>(null);
  const bodyHeight = useGridBodyHeight(gridRef);

  // The controller is rebuilt with the store; keep the latest without refetching on every render.
  const findRef = React.useRef(findFilterPage);
  findRef.current = findFilterPage;
  // Only the latest request updates the table, so a slow earlier response cannot overwrite it.
  const requestRef = React.useRef(0);

  React.useEffect(() => {
    const request = ++requestRef.current;
    setIsLoading(true);
    findRef
      .current({
        keyword: keyword.trim() || undefined,
        page: pageIndex + 1,
        quantity: pageSize,
        sort: sort.length ? sort : undefined,
      })
      .then((result) => {
        if (request === requestRef.current) setPage(result);
      })
      .catch(() => {})
      .finally(() => {
        if (request === requestRef.current) setIsLoading(false);
      });
  }, [keyword, pageIndex, pageSize, sort]);

  return (
    <styled.FilterList>
      <FormPage>
        <Row className="add-media" justifyContent="flex-end">
          <Col flex="1 1 0">A filter provides a way to save an Elasticsearch query.</Col>
          <IconButton
            iconType="plus"
            label={'Add new filter'}
            onClick={() => {
              navigate('/admin/filters/0');
            }}
          />
        </Row>
        <FilterAISettings />
        <ListFilter
          onSearch={(value) => {
            setPageIndex(0);
            setKeyword(value);
          }}
        />
        <div
          ref={gridRef}
          className="filter-grid"
          style={
            {
              '--grid-body-height': bodyHeight ? `${bodyHeight}px` : undefined,
            } as React.CSSProperties
          }
        >
          <Grid
            items={page.items}
            pageIndex={pageIndex}
            itemsPerPage={pageSize}
            totalItems={page.total}
            isLoading={isLoading}
            showPaging
            onNavigatePage={async (index) => setPageIndex(index)}
            onQuantityChange={async (quantity) => {
              setPageIndex(0);
              setPageSize(quantity);
            }}
            onSortChange={async (column, direction) => {
              setPageIndex(0);
              setSort(
                direction === SortDirection.None || !column.name
                  ? []
                  : [`${column.name}${direction === SortDirection.Descending ? ' desc' : ''}`],
              );
            }}
            renderHeader={() => [
              { name: 'name', label: 'Name', sortable: true },
              { name: 'description', label: 'Description', sortable: true },
              { name: 'owner.username', label: 'Owner', sortable: true },
              { name: 'keywords', label: 'Keywords', sortable: false },
              { name: 'isEnabled', label: 'Enabled', size: '120px', sortable: true },
            ]}
            renderColumns={(row: IFilterModel, rowIndex) => [
              <CellEllipsis key="1">
                <Link to={`/admin/filters/${row.id}`}>{row.name}</Link>
              </CellEllipsis>,
              <CellEllipsis key="2">{row.description}</CellEllipsis>,
              <CellEllipsis key="3">{row.owner?.username}</CellEllipsis>,
              <div key="4" className="keyword-cell">
                <CellEllipsis>{truncateString(row.settings?.search)}</CellEllipsis>
                {row.settings?.search ? (
                  <FaRegClipboard
                    className="clipboard-icon"
                    title="Copy keywords to clipboard"
                    onClick={(e) => {
                      e.stopPropagation();
                      handleCopyKeyWords(e, row.settings.search);
                    }}
                  />
                ) : null}
              </div>,
              <CellCheckbox key="5" checked={row.isEnabled} />,
            ]}
          />
        </div>
      </FormPage>
    </styled.FilterList>
  );
};

export default FilterList;
