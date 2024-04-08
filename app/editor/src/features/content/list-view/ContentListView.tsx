import { KnnSearchResponse, SearchTotalHits } from '@elastic/elasticsearch/lib/api/types';
import { NavigateOptions, useTab } from 'components/tab-control';
import moment from 'moment';
import React, { lazy } from 'react';
import { useParams } from 'react-router-dom';
import { useApiHub, useApp, useContent, useLocalStorage, useWorkOrders } from 'store/hooks';
import { IContentSearchResult } from 'store/slices';
import { useCastContentToSearchResult } from 'store/slices/content/hooks';
import {
  Col,
  ContentTypeName,
  FlexboxTable,
  IContentMessageModel,
  IContentModel,
  IPaged,
  ITableInternalRow,
  ITablePage,
  ITableSort,
  IWorkOrderFilter,
  IWorkOrderMessageModel,
  IWorkOrderModel,
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
  const castContentToSearchResult = useCastContentToSearchResult();

  const [contentId, setContentId] = React.useState(id);
  const [contentType, setContentType] = React.useState(formType ?? ContentTypeName.AudioVideo);
  const [isLoading, setIsLoading] = React.useState(false);

  const [, { findWorkOrders }] = useWorkOrders();
  // This configures the shared storage between this list and any content tabs
  // that are opened.  Mainly used for navigation in the tab
  const [, setCurrentItems] = useLocalStorage('currentContent', {} as IContentSearchResult[]);
  const [currentItemId, setCurrentItemId] = useLocalStorage('currentContentItemId', -1);

  // Stores the current page
  const [currentResultsPage, setCurrentResultsPage] = React.useState(defaultPage);
  React.useEffect(() => {
    setCurrentItems(currentResultsPage.items);
  }, [currentResultsPage, setCurrentItems]);
  // if the user navigates next/previous in another window change the highlighted row
  React.useEffect(() => {
    if (currentItemId !== -1) setContentId(currentItemId.toString());
  }, [currentItemId, setContentId]);

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
    [castContentToSearchResult, currentResultsPage, getContent],
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
    [userId, getContent, currentResultsPage, castContentToSearchResult],
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
    [castContentToSearchResult, currentResultsPage, getContent],
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

          let workOrders: IPaged<IWorkOrderModel> = {
            page: 0,
            quantity: 0,
            total: 0,
            items: [],
          };
          if (filter.pendingTranscript) {
            // Make a request for transcript work orders.
            const startDate = moment().add(-3, 'day').startOf('day');
            const endDate = moment().endOf('day');
            const woFilter: IWorkOrderFilter = {
              page: 1,
              quantity: 1000,
              createdStartOn: startDate.toISOString(),
              createdEndOn: endDate.toISOString(),
              workType: WorkOrderTypeName.Transcription,
              status: [
                WorkOrderStatusName.Submitted,
                WorkOrderStatusName.InProgress,
                WorkOrderStatusName.Completed,
                WorkOrderStatusName.Failed,
              ],
            };

            const response = await findWorkOrders(woFilter);
            workOrders = response.data;
            filter.contentIds = workOrders.items
              .filter((wo) => !!wo.contentId)
              .map((wo) => wo.contentId!);
          }

          const doSearch =
            !filter.pendingTranscript || (filter.pendingTranscript && workOrders.items.length);
          const searchResults: KnnSearchResponse<IContentModel> = doSearch
            ? await findContentWithElasticsearch(toFilter(filter), true)
            : ({
                hits: { hits: [], total: { value: 0 } },
              } as unknown as KnnSearchResponse<IContentModel>);
          let items = searchResults.hits?.hits?.map((h) =>
            castContentToSearchResult(h._source as IContentModel),
          );

          if (filter.pendingTranscript) {
            // Apply the transcript work order to the content.
            items = items
              .filter((content) => !content.isApproved)
              .map((content) => {
                const workOrder = workOrders.items.find((wo) => wo.contentId === content.id);
                return { ...content, transcriptStatus: workOrder?.status };
              });
          }

          const page = new Page(
            1,
            filter.pageSize,
            items,
            (searchResults.hits?.total as SearchTotalHits).value,
          );
          setCurrentResultsPage(page);
          return page;
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
    setCurrentItemId(row.original.id);
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
