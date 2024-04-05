import { SearchTotalHits } from '@elastic/elasticsearch/lib/api/types';
import { NavigateOptions, useTab } from 'components/tab-control';
import React from 'react';
import { useParams } from 'react-router-dom';
import { toast } from 'react-toastify';
import { useApiHub, useContent, useLocalStorage, useSettings } from 'store/hooks';
import { IContentSearchResult, storeContentFilterAdvanced } from 'store/slices';
import { useCastContentToSearchResult } from 'store/slices/content/hooks/useCastContentToSearchResult';
import {
  Col,
  ContentStatusName,
  FlexboxTable,
  IContentModel,
  ITableInternalCell,
  ITableInternalRow,
  ITablePage,
  ITableSort,
  MessageTargetName,
  Page,
  replaceQueryParams,
  Row,
} from 'tno-core';

import { AdvancedSearchKeys } from '../constants';
import { useElasticsearch } from '../hooks';
import { IContentListAdvancedFilter, IContentListFilter } from '../interfaces';
import { defaultPage } from '../list-view/constants';
import { queryToFilter, queryToFilterAdvanced } from '../list-view/utils';
import { ReportActions } from './components';
import { defaultPaperFilter, defaultSort, defaultTotals } from './constants';
import { useColumns, usePaperSources, useSortContent } from './hooks';
import { ITotalsInfo } from './interfaces';
import { PaperToolbar } from './PaperToolbar';
import * as styled from './styled';
import { calcTotals } from './utils';

export interface IPapersProps extends React.HTMLAttributes<HTMLDivElement> {}

/**
 * Provides a list view of print content to help select stories for the papers.
 * @param props Component props.
 * @returns Component.
 */
const Papers: React.FC<IPapersProps> = (props) => {
  const { id } = useParams();
  const [
    { filterPaper: filter, filterPaperAdvanced: filterAdvanced },
    { findContentWithElasticsearch, storeFilterPaper, updateContent: updateStatus, getContent },
  ] = useContent();
  const paperSources = usePaperSources();
  const { navigate } = useTab();
  const hub = useApiHub();
  const toFilter = useElasticsearch();
  const castContentToSearchResult = useCastContentToSearchResult();
  const { isReady: settingsReady } = useSettings(true);
  const sortContent = useSortContent();

  // This configures the shared storage between this list and any content tabs
  // that are opened.  Mainly used for navigation in the tab
  const [, setCurrentItems] = useLocalStorage('currentContent', {} as IContentSearchResult[]);
  const [currentItemId, setCurrentItemId] = useLocalStorage('currentContentItemId', -1);

  const [contentId, setContentId] = React.useState(id);
  const [isLoading, setIsLoading] = React.useState(false);
  const [isFilterLoading, setIsFilterLoading] = React.useState(true);
  const [selected, setSelected] = React.useState<IContentSearchResult[]>([]);
  const [currentResultsPage, setCurrentResultsPage] = React.useState(defaultPage);
  const [totals, setTotals] = React.useState<ITotalsInfo>(defaultTotals);

  const selectedIds = selected.map((i) => i.id.toString());

  hub.useHubEffect(MessageTargetName.ContentUpdated, (message) => {
    getContent(message.id)
      .then((content) => {
        if (content) {
          setCurrentResultsPage((page) => {
            const newPage = {
              ...page,
              items: page.items.map((q) =>
                q.id === content.id ? castContentToSearchResult(content) : q,
              ),
            };
            if (
              filter.topStory ||
              filter.commentary ||
              filter.featuredStory ||
              filter.onlyPublished
            ) {
              newPage.items = newPage.items.filter((i) =>
                filter.topStory
                  ? i.isTopStory
                  : true && filter.commentary
                  ? i.isCommentary
                  : true && filter.featuredStory
                  ? i.isFeaturedStory
                  : true && filter.onlyPublished
                  ? [ContentStatusName.Publish, ContentStatusName.Published].includes(i.status)
                  : true,
              );
            }
            setTotals((values) => calcTotals(newPage.items, filter, values));
            return newPage;
          });
        }
      })
      .catch(() => {});
  });

  // Stores the current page
  React.useEffect(() => {
    setCurrentItems(currentResultsPage.items);
  }, [currentResultsPage, setCurrentItems]);

  // if the user navigates next/previous in another window change the highlighted row
  React.useEffect(() => {
    if (currentItemId !== -1) setContentId(currentItemId.toString());
  }, [currentItemId, setContentId]);

  const handleClickUse = React.useCallback(
    (content: IContentSearchResult) => {
      updateStatus({ ...content.original, status: content.status })
        .then((content) => {
          setCurrentResultsPage((page) => ({
            ...page,
            items: page.items.map((i) =>
              i.id === content.id ? castContentToSearchResult(content) : i,
            ),
          }));
          toast.success(
            `"${content.headline}" has been updated.  A request has been sent to update the index.`,
          );
        })
        .catch((error) => {});
    },
    [castContentToSearchResult, updateStatus],
  );

  const fetch = React.useCallback(
    async (filter: IContentListFilter & Partial<IContentListAdvancedFilter>) => {
      try {
        setIsLoading(true);
        const results = await findContentWithElasticsearch(
          toFilter({
            ...filter,
          }),
          true,
        );
        const items = results.hits.hits
          .filter((h) => !!h._source)
          .map((h) => h._source! as IContentModel)
          .sort(sortContent(filter.sort))
          .map((c) => castContentToSearchResult(c));
        const page = new Page(
          1,
          filter.pageSize,
          items,
          (results.hits?.total as SearchTotalHits).value,
        );
        setCurrentResultsPage(page);
        setTotals((values) => calcTotals(page.items, filter, values));
        return page;
      } catch (error) {
        throw error;
      } finally {
        setIsLoading(false);
      }
    },
    [castContentToSearchResult, findContentWithElasticsearch, toFilter, sortContent],
  );

  const columns = useColumns({ fetch, onClickUse: handleClickUse });

  React.useEffect(() => {
    // Extract query string values and place them into redux store.
    if (!window.location.search) {
      replaceQueryParams(defaultPaperFilter(paperSources), { includeEmpty: false });
    }
    storeFilterPaper({
      ...queryToFilter(
        {
          ...defaultPaperFilter(paperSources),
        },
        window.location.search,
      ),
    });
    storeContentFilterAdvanced(
      queryToFilterAdvanced(
        { ...filterAdvanced, fieldType: AdvancedSearchKeys.Headline },
        window.location.search,
      ),
    );
    setIsFilterLoading(false);
    // Only want this to run on the first load.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  React.useEffect(() => {
    // Do not want to fetch while the default paper filter is still loading
    if (!isFilterLoading && settingsReady) fetch({ ...filter, ...filterAdvanced });
    // Do not want to fetch when the advanced filter changes.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [filter, isFilterLoading, settingsReady]);

  const handleRowClick = (
    cell: ITableInternalCell<IContentSearchResult>,
    row: ITableInternalRow<IContentSearchResult>,
    event: React.MouseEvent<Element, MouseEvent>,
  ) => {
    if (cell.index > 0 && cell.index !== 5) {
      setContentId(row.original.id.toString());
      setCurrentItemId(row.original.id);
      if (event.ctrlKey) navigate(row.original.id, '/contents', NavigateOptions.NewTab);
      else navigate(row.original.id);
    }
  };

  const handleChangePage = React.useCallback(
    (page: ITablePage) => {
      if (filter.pageIndex !== page.pageIndex || filter.pageSize !== page.pageSize) {
        const newFilter = {
          ...filter,
          pageIndex: page.pageIndex,
          pageSize: page.pageSize ?? filter.pageSize,
        };
        storeFilterPaper(newFilter);
        replaceQueryParams(newFilter, { includeEmpty: false });
      }
    },
    [filter, storeFilterPaper],
  );

  const handleChangeSort = React.useCallback(
    (sort: ITableSort<IContentSearchResult>[]) => {
      const sorts = sort.filter((s) => s.isSorted).map((s) => ({ id: s.id, desc: s.isSortedDesc }));
      storeFilterPaper({ ...filter, sort: sorts.length ? sorts : defaultSort });
    },
    [filter, storeFilterPaper],
  );

  const handleSelectedRowsChanged = (row: ITableInternalRow<IContentSearchResult>) => {
    if (row.isSelected) {
      setSelected(row.table.rows.filter((r) => r.isSelected).map((r) => r.original));
    } else {
      setSelected((selected) => selected.filter((r) => r.id !== row.original.id));
    }
  };

  const handleContentHidden = React.useCallback(
    async (contentToHide: IContentModel[]) => {
      try {
        let filteredItems = currentResultsPage.items;
        contentToHide.forEach((c) => {
          filteredItems = currentResultsPage.items.filter((i) => i.id !== c.id);
        });
        const newPage = {
          ...currentResultsPage,
          items: filteredItems,
        };
        setCurrentResultsPage(newPage);
      } catch {}
    },
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [currentResultsPage],
  );

  return (
    <styled.Papers>
      <Col wrap="nowrap">
        <PaperToolbar onSearch={fetch} />
        <div className="paper-totals">
          <div>
            <div>Top Stories:</div>
            <div>{totals.topStories}</div>
          </div>
          <div>
            <div>Commentary:</div>
            <div>{totals.commentary}</div>
          </div>
          <div>
            <div>Featured Stories:</div>
            <div>{totals.featuredStories}</div>
          </div>
          <div>
            <div>Published:</div>
            <div>{totals.published}</div>
          </div>
        </div>
        <Row className="content-list">
          <FlexboxTable
            rowId="id"
            columns={columns}
            data={currentResultsPage.items}
            isMulti={true}
            manualPaging={true}
            pageIndex={filter.pageIndex}
            pageSize={filter.pageSize}
            pageCount={currentResultsPage.pageCount}
            totalItems={currentResultsPage.total}
            showSort={true}
            activeRowId={contentId}
            isLoading={isLoading || isFilterLoading}
            onPageChange={handleChangePage}
            onSortChange={handleChangeSort}
            onCellClick={handleRowClick}
            onSelectedChanged={handleSelectedRowsChanged}
            selectedRowIds={selectedIds}
          />
        </Row>
        <ReportActions
          setLoading={setIsLoading}
          selected={selected}
          searchResults={currentResultsPage}
          filter={filter}
          onContentHidden={handleContentHidden}
        />
      </Col>
    </styled.Papers>
  );
};

export default Papers;
