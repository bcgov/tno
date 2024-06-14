import { KnnSearchResponse, SearchTotalHits } from '@elastic/elasticsearch/lib/api/types';
import { Status } from 'components/status';
import { NavigateOptions, TabControl, useTab } from 'components/tab-control';
import moment from 'moment';
import React, { lazy, useRef } from 'react';
import { useParams } from 'react-router-dom';
import { toast } from 'react-toastify';
import { useApiHub, useApp, useContent, useLocalStorage, useWorkOrders } from 'store/hooks';
import { IContentSearchResult } from 'store/slices';
import { useCastContentToSearchResult } from 'store/slices/content/hooks';
import {
  CellEllipsis,
  Checkbox,
  Col,
  ContentTypeName,
  Grid,
  IContentMessageModel,
  IContentModel,
  IGridHeaderColumnProps,
  IPaged,
  IWorkOrderFilter,
  IWorkOrderMessageModel,
  IWorkOrderModel,
  LogicalOperator,
  MessageTargetName,
  Page,
  replaceQueryParams,
  Row,
  Show,
  SortDirection,
  useCombinedView,
  WorkOrderStatusName,
  WorkOrderTypeName,
} from 'tno-core';

import { AdvancedSearchKeys } from '../constants';
import { useElasticsearch } from '../hooks';
import { IContentListAdvancedFilter, IContentListFilter } from '../interfaces';
import { ContentToolBar } from './components';
import { defaultPage } from './constants';
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
    {
      getContent,
      storeFilter,
      storeFilterAdvanced,
      findContentWithElasticsearch,
      updateContent: updateStatus,
    },
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

  const [focusedRowIndex, setFocusedRowIndex] = React.useState<number | null>(null);
  const rowRefs = useRef<(HTMLDivElement | null)[]>([]);

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
      // add new unfiltered content to the search results
      // if it was created by the current user
      if (message.ownerId === userId) {
        try {
          const result = await getContent(message.id);
          if (!!result) {
            const newPage = {
              ...currentResultsPage,
              items: [...[castContentToSearchResult(result)], ...currentResultsPage.items],
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

  React.useEffect(() => {
    if (isReady) {
      fetch({ ...filter, ...filterAdvanced });
    }
    // Do not want to fetch when the advanced filter changes.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [isReady, filter, fetch]);

  const handlePageChange = React.useCallback(
    (page: number) => {
      if (filter.pageIndex !== page - 1) {
        const newFilter = {
          ...filter,
          pageIndex: page - 1,
        };
        storeFilter(newFilter);
        replaceQueryParams(newFilter, { includeEmpty: false });
      }
    },
    [filter, storeFilter],
  );

  const handleSortChange = React.useCallback(
    (column: IGridHeaderColumnProps, direction: SortDirection) => {
      if (column.name) {
        const sort =
          direction === SortDirection.None
            ? []
            : [{ id: column.name, desc: direction === SortDirection.Descending }];
        storeFilter({ ...filter, sort });
      }
    },
    [filter, storeFilter],
  );

  const handleContentClick = (id: number, event: React.MouseEvent<Element, MouseEvent>) => {
    setContentId(id.toString());
    setCurrentItemId(id);
    if (event.ctrlKey) navigate(id, '/contents', NavigateOptions.NewTab);
    else navigate(id);
  };

  React.useEffect(() => {
    setFocusedRowIndex(currentResultsPage?.items[0]?.id);
  }, [currentResultsPage]);

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

  React.useEffect(() => {
    if (focusedRowIndex !== null) {
      const focusedRow = currentResultsPage.items.findIndex((item) => item.id === focusedRowIndex);
      if (rowRefs.current[focusedRow]) {
        rowRefs.current[focusedRow]?.scrollIntoView({ behavior: 'smooth', block: 'center' });
      }
    }
  }, [focusedRowIndex, currentResultsPage.items]);

  const handleKeyDown = React.useCallback(
    (e: KeyboardEvent) => {
      if ((e.ctrlKey && e.key === 'ArrowUp') || (e.ctrlKey && e.key === 'ArrowDown')) {
        e.preventDefault();
        setFocusedRowIndex((prevIndex) => {
          let currentIndex = currentResultsPage.items.findIndex((item) => item.id === prevIndex);
          if (currentIndex === -1) currentIndex = 0;

          let newIndex = currentIndex;

          if (e.key === 'ArrowUp') {
            newIndex = currentIndex - 1;
          } else if (e.key === 'ArrowDown') {
            newIndex = currentIndex + 1;
          }

          if (newIndex < 0) {
            newIndex = currentResultsPage.items.length - 1;
          } else if (newIndex >= currentResultsPage.items.length) {
            newIndex = 0;
          }

          return currentResultsPage.items[newIndex].id;
        });
      }
    },
    [currentResultsPage.items],
  );

  React.useEffect(() => {
    window.addEventListener('keydown', handleKeyDown);
    return () => {
      window.removeEventListener('keydown', handleKeyDown);
    };
  }, [handleKeyDown]);

  return (
    <styled.ContentListView>
      <Col wrap="nowrap">
        <ContentToolBar onSearch={fetch} />
        <Row className="top-pane">
          <Row className="content-list">
            <Grid
              items={currentResultsPage.items}
              pageIndex={currentResultsPage.pageIndex - 1}
              itemsPerPage={currentResultsPage.pageSize}
              totalItems={currentResultsPage.total}
              showPaging
              onNavigatePage={async (page) => {
                handlePageChange(page);
              }}
              onSortChange={async (column, direction) => {
                handleSortChange(column, direction);
              }}
              renderHeader={() => [
                {
                  name: 'headline',
                  label: (
                    <Row gap="0.5rem" key="">
                      <TabControl />
                      Headline
                    </Row>
                  ),
                  size: '40%',
                },
                { name: 'otherSource', label: 'Source', sortable: true, size: '15%' },
                { name: 'mediaTypeId', label: 'Media Type', sortable: true, size: '15%' },
                {
                  name: 'page',
                  label: (
                    <Row nowrap key="">
                      Page:Section
                      <Checkbox
                        name="page"
                        checked={
                          filterAdvanced.fieldType === AdvancedSearchKeys.Page &&
                          filterAdvanced.searchTerm === '?*'
                        }
                        onChange={async (e) => {
                          const values = {
                            ...filterAdvanced,
                            fieldType: AdvancedSearchKeys.Page,
                            searchTerm: e.target.checked ? '?*' : '',
                            logicalOperator: LogicalOperator.Equals,
                          };
                          storeFilterAdvanced(values);
                          await fetch({ ...filter, ...values });
                        }}
                      />
                    </Row>
                  ),
                  sortable: true,
                },
                { name: 'owner', label: 'User', sortable: true },
                { name: 'publishedOn', label: 'Pub Date', sortable: true, size: '10%' },
                { name: 'status', label: 'Use', sortable: true, size: '75px' },
              ]}
              renderColumns={(row: IContentSearchResult, rowIndex) => {
                const isFocused = focusedRowIndex === row.id;
                const separator = row.page && row.section ? ':' : '';
                const pageSection = `${row.page}${separator}${row.section}`;

                return [
                  {
                    column: (
                      <div
                        key=""
                        ref={(el) => {
                          if (rowIndex) rowRefs.current[rowIndex] = el;
                        }}
                        className="clickable"
                        onClick={(e) => handleContentClick(row.id, e)}
                      >
                        <CellEllipsis>{row.headline}</CellEllipsis>
                      </div>
                    ),
                    isFocused: isFocused,
                  },
                  {
                    column: (
                      <div
                        className="clickable"
                        key=""
                        ref={(el) => {
                          if (rowIndex) rowRefs.current[rowIndex] = el;
                        }}
                        onClick={(e) => handleContentClick(row.id, e)}
                      >
                        <CellEllipsis>{row.otherSource}</CellEllipsis>
                      </div>
                    ),
                    isFocused: isFocused,
                  },
                  {
                    column: (
                      <div
                        className="clickable"
                        key=""
                        ref={(el) => {
                          if (rowIndex) rowRefs.current[rowIndex] = el;
                        }}
                        onClick={(e) => handleContentClick(row.id, e)}
                      >
                        <CellEllipsis
                          className="clickable"
                          key=""
                          onClick={(e) => handleContentClick(row.id, e)}
                        >
                          {row.mediaType}
                        </CellEllipsis>
                      </div>
                    ),
                    isFocused: isFocused,
                  },
                  {
                    column: (
                      <div
                        className="clickable"
                        key=""
                        ref={(el) => {
                          if (rowIndex) rowRefs.current[rowIndex] = el;
                        }}
                        onClick={(e) => handleContentClick(row.id, e)}
                      >
                        {pageSection}
                      </div>
                    ),
                    isFocused: isFocused,
                  },
                  {
                    column: (
                      <div
                        className="clickable"
                        key=""
                        ref={(el) => {
                          if (rowIndex) rowRefs.current[rowIndex] = el;
                        }}
                        onClick={(e) => handleContentClick(row.id, e)}
                      >
                        {row.owner}
                      </div>
                    ),
                    isFocused: isFocused,
                  },
                  {
                    column: (
                      <div
                        className="clickable"
                        key=""
                        ref={(el) => {
                          if (rowIndex) rowRefs.current[rowIndex] = el;
                        }}
                        onClick={(e) => handleContentClick(row.id, e)}
                      >
                        {row.publishedOn
                          ? moment(row.publishedOn).format('MM/DD/YYYY HH:mm:ss')
                          : ''}
                      </div>
                    ),
                    isFocused: isFocused,
                  },
                  {
                    column: (
                      <div
                        key=""
                        ref={(el) => {
                          if (rowIndex) rowRefs.current[rowIndex] = el;
                        }}
                        className="clickable"
                      >
                        <Status
                          value={row.status}
                          onClick={(status) => handleClickUse({ ...row, status: status })}
                        />
                      </div>
                    ),
                    isFocused: isFocused,
                  },
                ];
              }}
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
