import { SearchTotalHits } from '@elastic/elasticsearch/lib/api/types';
import { NavigateOptions, useTab } from 'components/tab-control';
import React, { lazy } from 'react';
import { useParams } from 'react-router-dom';
import { useApiHub, useApp, useContent, useLocalStorage, useWorkOrders } from 'store/hooks';
import { IContentSearchResult } from 'store/slices';
import { castContentToSearchResult } from 'store/slices/content/utils';
import {
  Col,
  ContentTypeName,
  FlexboxTable,
  IContentMessageModel,
  IContentModel,
  ITableInternalRow,
  ITablePage,
  ITableSort,
  IWorkOrderFilter,
  IWorkOrderMessageModel,
  MessageTargetName,
  Page,
  replaceQueryParams,
  Row,
  Show,
  useCombinedView,
  WorkOrderStatusName,
  WorkOrderTypeName,
} from 'tno-core';

import { useElasticsearch } from '../hooks';
import { IContentListAdvancedFilter, IContentListFilter } from '../interfaces';
import { ContentToolBar } from './components';
import { defaultPage } from './constants';
import { useColumns } from './hooks';
import * as styled from './styled';
import { queryToFilter, queryToFilterAdvanced } from './utils';

const ContentForm = lazy(() => import('../form/ContentForm'));

/**
 * ContentListView provides a way to list, search and select content for viewing and editing.
 * Also provides a combined view which splits the page into two columns.
 * @returns Component
 */
const ContentListView: React.FC = () => {
  const [{ userInfo }] = useApp();
  const { id } = useParams();
  const [
    { filter, filterAdvanced },
    { getContent, storeFilter, storeFilterAdvanced, findContentWithElasticsearch },
  ] = useContent();
  const { combined, formType } = useCombinedView();
  const { navigate } = useTab({ showNav: false });
  const hub = useApiHub();
  const toFilter = useElasticsearch();

  const [contentId, setContentId] = React.useState(id);
  const [contentType, setContentType] = React.useState(formType ?? ContentTypeName.AudioVideo);
  const [isLoading, setIsLoading] = React.useState(false);

  const [, { findWorkOrders }] = useWorkOrders();
  // This configures the shared storage between this list and any content tabs
  // that are opened.  Mainly used for navigation in the tab
  const [, setCurrentItems] = useLocalStorage('currentContent', {} as IContentSearchResult[]);

  // Stores the current page
  const [currentResultsPage, setCurrentResultsPage] = React.useState(defaultPage);
  React.useEffect(() => {
    setCurrentItems(currentResultsPage.items);
  }, [currentResultsPage, setCurrentItems]);

  React.useEffect(() => {
    // Extract query string values and place them into redux store.
    if (!!window.location.search) {
      storeFilter(queryToFilter(filter, window.location.search));
      storeFilterAdvanced(queryToFilterAdvanced(filterAdvanced, window.location.search));
    }
    // Only want this to run on the first load.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const userId = userInfo?.id ?? '';
  const isReady = !!userId && filter.userId !== '';

  const onWorkOrder = React.useCallback(
    (workOrder: IWorkOrderMessageModel) => {
      if (
        [WorkOrderTypeName.Transcription, WorkOrderTypeName.NaturalLanguageProcess].includes(
          workOrder.workType,
        )
      ) {
        if (!!workOrder.contentId) {
          getContent(workOrder.contentId ?? 0).then((content) => {
            if (!!content) {
              const newPage = {
                ...currentResultsPage,
                items: currentResultsPage.items.map((i) => {
                  if (i.id === content.id) {
                    return castContentToSearchResult(content);
                  } else {
                    return i;
                  }
                }),
              };
              setCurrentResultsPage(newPage);
            }
          });
        }
      }
    },
    [currentResultsPage, getContent],
  );
  hub.useHubEffect(MessageTargetName.WorkOrder, onWorkOrder);

  const onContentAdded = React.useCallback(
    async (message: IContentMessageModel) => {
      if (message.ownerId === userId) {
        try {
          const result = await getContent(message.id);
          if (!!result) {
            const newPage = {
              ...currentResultsPage,
              items: currentResultsPage.items.map((i) => {
                if (i.id === result.id) {
                  return castContentToSearchResult(result);
                } else {
                  return i;
                }
              }),
            };
            setCurrentResultsPage(newPage);
          }
        } catch {}
      }
    },
    [userId, currentResultsPage, getContent],
  );
  hub.useHubEffect(MessageTargetName.ContentAdded, onContentAdded);

  const onContentUpdated = React.useCallback(
    async (message: IContentMessageModel) => {
      // Only update if the current page includes the updated content.
      if (currentResultsPage.items.some((c) => c.id === message.id)) {
        try {
          const result = await getContent(message.id);
          if (!!result) {
            const newPage = {
              ...currentResultsPage,
              items: currentResultsPage.items.map((i) => {
                if (i.id === result.id) {
                  return castContentToSearchResult(result);
                } else {
                  return i;
                }
              }),
            };
            setCurrentResultsPage(newPage);
          }
        } catch {}
      }
    },
    [currentResultsPage, getContent],
  );
  hub.useHubEffect(MessageTargetName.ContentUpdated, onContentUpdated);

  const onContentDeleted = React.useCallback(
    async (message: IContentMessageModel) => {
      // Only update if the current page includes the updated content.
      if (currentResultsPage.items.some((c) => c.id === message.id)) {
        try {
          const newPage = {
            ...currentResultsPage,
            items: currentResultsPage.items.filter((i) => i.id !== message.id),
          };
          setCurrentResultsPage(newPage);
        } catch {}
      }
    },
    [currentResultsPage],
  );
  hub.useHubEffect(MessageTargetName.ContentDeleted, onContentDeleted);

  React.useEffect(() => {
    // Required because the first time this page is loaded directly the user has not been set.
    // Don't make a request until the user has been set.
    if (userId !== '' && filter.userId === '') {
      storeFilter({ ...filter, userId });
    }
  }, [userId, filter, storeFilter]);

  const fetch = React.useCallback(
    async (filter: IContentListFilter & Partial<IContentListAdvancedFilter>) => {
      try {
        if (!isLoading) {
          setIsLoading(true);

          const endDate = new Date();
          const startDate = new Date();
          startDate.setDate(endDate.getDate() - 3);
          const woFilter: IWorkOrderFilter = {
            createdStartOn: startDate.toLocaleDateString('en-US'),
            createdEndOn: endDate.toLocaleDateString('en-US'),
            status: WorkOrderStatusName.InProgress, // kgm: bring back only what we are interested in
          };

          const [results, workOrders] = await Promise.all([
            findContentWithElasticsearch(toFilter(filter), true),
            findWorkOrders(woFilter),
          ]);

          const items = results.hits?.hits?.map((h) =>
            castContentToSearchResult(h._source as IContentModel),
          );

          if (filter.pendingTranscript && items) {
            let itemsWithStatus: IContentSearchResult[] | undefined = items.map((m) => {
              let v = { ...m };
              v.transcriptStatus = workOrders.data.items.find((w) => w.contentId === m.id)?.status;
              return v;
            });
            if (itemsWithStatus) {
              itemsWithStatus = itemsWithStatus.filter(
                (x) => x.transcriptStatus === WorkOrderStatusName.InProgress,
              );
            }
            if (results && itemsWithStatus) {
              const page = new Page(1, filter.pageSize, itemsWithStatus, itemsWithStatus.length);
              setCurrentResultsPage(page);
              return page;
            } else {
              setCurrentResultsPage(defaultPage);
              return defaultPage as Page<IContentSearchResult>;
            }
          } else {
            const page = new Page(
              1,
              filter.pageSize,
              items,
              (results.hits?.total as SearchTotalHits).value,
            );
            setCurrentResultsPage(page);
            return page;
          }
        }
      } catch {
      } finally {
        setIsLoading(false);
      }
    },
    // 'isLoading' will result in an infinite loop for some reason.
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [findContentWithElasticsearch, toFilter],
  );

  const columns = useColumns({ fetch });

  React.useEffect(() => {
    if (isReady) {
      fetch({ ...filter, ...filterAdvanced });
    }
    // Do not want to fetch when the advanced filter changes.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [isReady, filter, fetch]);

  const handleChangePage = React.useCallback(
    (page: ITablePage) => {
      if (filter.pageIndex !== page.pageIndex || filter.pageSize !== page.pageSize) {
        const newFilter = {
          ...filter,
          pageIndex: page.pageIndex,
          pageSize: page.pageSize ?? filter.pageSize,
        };
        storeFilter(newFilter);
        replaceQueryParams(newFilter, { includeEmpty: false });
      }
    },
    [filter, storeFilter],
  );

  const handleChangeSort = React.useCallback(
    (sort: ITableSort<IContentSearchResult>[]) => {
      const sorts = sort.filter((s) => s.isSorted).map((s) => ({ id: s.id, desc: s.isSortedDesc }));
      storeFilter({ ...filter, sort: sorts });
    },
    [storeFilter, filter],
  );

  const handleRowClick = (
    row: ITableInternalRow<IContentSearchResult>,
    event: React.MouseEvent<Element, MouseEvent>,
  ) => {
    setContentType(row.original.contentType);
    setContentId(row.original.id.toString());
    if (event.ctrlKey) navigate(row.original.id, '/contents', NavigateOptions.NewTab);
    else navigate(row.original.id);
  };

  const handleKeyDown = (event: React.KeyboardEvent<HTMLDivElement>) => {
    if (event.key === 'ArrowUp' || event.key === 'ArrowDown') {
      const index = !!contentId
        ? currentResultsPage?.items.findIndex((c: any) => c.id === +contentId) ?? -1
        : -1;
      const newIndex = event.key === 'ArrowUp' ? index - 1 : index + 1;
      const newContent = currentResultsPage.items[newIndex];
      if (newContent) {
        setContentType(newContent.contentType);
        setContentId(newContent.id.toString());
        const currentRow = document.querySelector('div.active');
        event.key === 'ArrowUp'
          ? (currentRow?.previousSibling as any)?.focus()
          : (currentRow?.nextSibling as any)?.focus();
      }
    }
  };

  return (
    <styled.ContentListView>
      <Col wrap="nowrap">
        <ContentToolBar onSearch={fetch} />
        <Row className="top-pane">
          <Row className="content-list">
            <FlexboxTable
              rowId="id"
              columns={columns}
              data={currentResultsPage.items}
              manualPaging={true}
              pageIndex={filter.pageIndex}
              pageSize={filter.pageSize}
              pageCount={currentResultsPage.pageCount}
              totalItems={currentResultsPage.total}
              showSort={true}
              activeRowId={contentId}
              isLoading={isLoading}
              onPageChange={handleChangePage}
              onSortChange={handleChangeSort}
              onRowClick={handleRowClick}
              onKeyDown={handleKeyDown}
            />
          </Row>
        </Row>
        <Show visible={combined}>
          <hr />
          <Row className="bottom-pane" id="bottom-pane">
            <ContentForm contentType={contentType} />
          </Row>
        </Show>
      </Col>
    </styled.ContentListView>
  );
};

export default ContentListView;
