import { SearchTotalHits } from '@elastic/elasticsearch/lib/api/types';
import { NavigateOptions, useTab } from 'components/tab-control';
import React from 'react';
import { flushSync } from 'react-dom';
import { useParams } from 'react-router-dom';
import { toast } from 'react-toastify';
import { useApiHub, useApp, useContent, useLocalStorage, useLookup } from 'store/hooks';
import { IContentSearchResult, storeContentFilterAdvanced } from 'store/slices';
import { castContentToSearchResult } from 'store/slices/content/utils';
import {
  Col,
  FlexboxTable,
  IContentMessageModel,
  IContentModel,
  IPage,
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
import { defaultPaperFilter } from './constants';
import { useColumns } from './hooks';
import { PaperToolbar } from './PaperToolbar';
import * as styled from './styled';

export interface IPapersProps extends React.HTMLAttributes<HTMLDivElement> {}

/**
 * Provides a list view of print content to help select stories for the papers.
 * @param props Component props.
 * @returns Component.
 */
const Papers: React.FC<IPapersProps> = (props) => {
  const [{ userInfo }] = useApp();
  const { id } = useParams();
  const [
    { filterPaper: filter, filterPaperAdvanced: filterAdvanced },
    { findContentWithElasticsearch, storeFilterPaper, updateContent: updateStatus, getContent },
  ] = useContent();
  const [{ sources }] = useLookup();

  const { navigate } = useTab();
  const hub = useApiHub();
  const toFilter = useElasticsearch();

  const [contentId, setContentId] = React.useState(id);

  const [isLoading, setIsLoading] = React.useState(false);
  const [isFilterLoading, setIsFilterLoading] = React.useState(true);
  const [selected, setSelected] = React.useState<IContentSearchResult[]>([]);
  const selectedIds = selected.map((i) => i.id.toString());
  const userId = userInfo?.id ?? '';

  // This configures the shared storage between this list and any content tabs
  // that are opened.  Mainly used for navigation in the tab
  const [, setCurrentItems] = useLocalStorage('currentContent', {} as IContentSearchResult[]);

  // Stores the current page
  const [currentResultsPage, setCurrentResultsPage] = React.useState(defaultPage);
  React.useEffect(() => {
    setCurrentItems(currentResultsPage.items);
  }, [currentResultsPage, setCurrentItems]);

  // Message process related states & logic:
  const [isProcessingMessages, setIsProcessingMessages] = React.useState<boolean>(false);
  const [contentUpdatesQueue, setContentUpdatesQueue] = React.useState<IContentMessageModel[]>([]);
  const [delayProcessing, setDelayProcessing] = React.useState<boolean>(false);

  // flushSync ensures that message queues are updated immediately, so messages are never lost
  hub.useHubEffect(MessageTargetName.ContentAdded, (message) => {
    if (message.ownerId === userId) {
      flushSync(() => {
        setContentUpdatesQueue((queue) => [...queue, message]);
      });
    }
  });

  hub.useHubEffect(MessageTargetName.ContentUpdated, (message) => {
    flushSync(() => {
      setContentUpdatesQueue((queue) => [...queue, message]);
    });
  });

  React.useEffect(() => {
    if (!isProcessingMessages && !delayProcessing && contentUpdatesQueue?.length) {
      setDelayProcessing(true); // delays processing too soon, allows msgs to queue up
      setIsProcessingMessages(true);
      processContentUpdates(contentUpdatesQueue);
    }
    // this timeout allows queue messages to be batched if they appear in
    // quick succession, e.g. less than 200ms between messages.
    const timer = setTimeout(() => {
      setDelayProcessing(false);
    }, 200);
    return () => clearTimeout(timer);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [contentUpdatesQueue, isProcessingMessages, delayProcessing]);

  const updateCurrentResultsPage = (newPage: IPage<IContentSearchResult>, forceUpdate = false) => {
    if ((!isProcessingMessages || forceUpdate) && newPage) {
      setCurrentResultsPage(newPage);
    }
  };

  // Process messages in the queue, fetching updated content data is neccesary,
  // finally it updates the content results in the page to reflect the updated data.
  const processContentUpdates = async (messages: IContentMessageModel[]) => {
    if (!isProcessingMessages && messages?.length) {
      const processedMessages: number[] = [];
      const updatedItems: IContentSearchResult[] = [];
      const resultsPage = currentResultsPage;

      // we must await otherwise we end up with fractured state updates
      await messages.reduce(async (promise, message: IContentMessageModel) => {
        await promise;
        if (processedMessages.includes(message.id)) return;

        processedMessages.push(message.id);
        const containsItem = resultsPage?.items?.some((c) => c.id === message?.id);
        if (containsItem) {
          try {
            const result = await getContent(message.id);
            if (!!result) {
              updatedItems.push(castContentToSearchResult(result));
            }
          } catch {}
        }
      }, Promise.resolve());

      if (updatedItems?.length) {
        const newPage = {
          ...resultsPage,
          items: resultsPage.items.map((i) => {
            const index = updatedItems.findIndex((updatedItem) => updatedItem.id === i.id);
            if (index > -1) {
              return updatedItems[index];
            } else {
              return i;
            }
          }),
        };
        updateCurrentResultsPage(newPage, true);
      }

      setContentUpdatesQueue((queue) => queue.filter((q) => !processedMessages.includes(q.id)));
      setIsProcessingMessages(false);
    }
  };

  const handleClickUse = React.useCallback(
    (content: IContentSearchResult) => {
      getContent(content.id)
        .then((response) => {
          if (response) {
            return updateStatus({ ...response, status: content.status }).then((content) => {
              setContentUpdatesQueue((queue) => [...queue, content]);
              toast.success(
                `"${content.headline}" has been updated.  A request has been sent to update the index.`,
              );
            });
          } else Promise.reject('Content does not exist');
        })
        .catch((error) => {});
    },
    [getContent, updateStatus],
  );

  const fetch = React.useCallback(
    async (filter: IContentListFilter & Partial<IContentListAdvancedFilter>) => {
      try {
        setIsLoading(true);
        const results = await findContentWithElasticsearch(toFilter(filter), true);
        const items = results.hits.hits
          .filter((h) => !!h._source)
          .map((h) => castContentToSearchResult(h._source! as IContentModel));
        const page = new Page(
          1,
          filter.pageSize,
          items,
          (results.hits?.total as SearchTotalHits).value,
        );
        updateCurrentResultsPage(page);
        return page;
      } catch (error) {
        // TODO: Handle error
        throw error;
      } finally {
        setIsLoading(false);
      }
    },
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [findContentWithElasticsearch, toFilter],
  );

  const columns = useColumns({ fetch, onClickUse: handleClickUse });

  React.useEffect(() => {
    // Extract query string values and place them into redux store.
    if (!window.location.search) {
      replaceQueryParams(defaultPaperFilter(sources), { includeEmpty: false });
    }
    storeFilterPaper({
      ...queryToFilter(
        {
          ...defaultPaperFilter(sources),
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
    if (isFilterLoading) return;
    if (isProcessingMessages) return;
    fetch({ ...filter, ...filterAdvanced });
    // Do not want to fetch when the advanced filter changes.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [filter, isFilterLoading]);

  const handleRowClick = (
    cell: ITableInternalCell<IContentSearchResult>,
    row: ITableInternalRow<IContentSearchResult>,
    event: React.MouseEvent<Element, MouseEvent>,
  ) => {
    if (cell.index > 0 && cell.index !== 5) {
      setContentId(row.original.id.toString());
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
      storeFilterPaper({ ...filter, sort: sorts });
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
      if (isProcessingMessages) return;
      try {
        let filteredItems = currentResultsPage.items;
        contentToHide.forEach((c) => {
          filteredItems = currentResultsPage.items.filter((i) => i.id !== c.id);
        });
        const newPage = {
          ...currentResultsPage,
          items: filteredItems,
        };
        updateCurrentResultsPage(newPage);
      } catch {}
    },
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [currentResultsPage, isProcessingMessages],
  );

  return (
    <styled.Papers>
      <Col wrap="nowrap">
        <PaperToolbar onSearch={fetch} />
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
