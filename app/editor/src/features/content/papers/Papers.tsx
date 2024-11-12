import { SearchTotalHits } from '@elastic/elasticsearch/lib/api/types';
import { Status } from 'components/status';
import { NavigateOptions, TabControl, useTab } from 'components/tab-control';
import React, { useRef } from 'react';
import { useParams } from 'react-router-dom';
import { toast } from 'react-toastify';
import { useApiHub, useApp, useContent, useLocalStorage, useSettings } from 'store/hooks';
import { IContentSearchResult, storeContentFilterAdvanced } from 'store/slices';
import { useCastContentToSearchResult } from 'store/slices/content/hooks/useCastContentToSearchResult';
import {
  CellEllipsis,
  Checkbox,
  Col,
  ContentStatusName,
  Grid,
  IContentModel,
  IGridHeaderColumnProps,
  LogicalOperator,
  MessageTargetKey,
  Page,
  replaceQueryParams,
  Row,
  SortDirection,
} from 'tno-core';

import { AdvancedSearchKeys } from '../constants';
import { useElasticsearch } from '../hooks';
import { IContentListAdvancedFilter, IContentListFilter } from '../interfaces';
import { defaultPage } from '../list-view/constants';
import { queryToFilter, queryToFilterAdvanced } from '../list-view/utils';
import { ReportActions } from './components';
import { defaultPaperFilter, defaultTotals } from './constants';
import { usePaperSources, useSortContent } from './hooks';
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
    {
      findContentWithElasticsearch,
      storeFilterPaper,
      updateContent: updateStatus,
      getContent,
      storeFilterPaperAdvanced,
    },
  ] = useContent();
  const paperSources = usePaperSources();
  const { navigate } = useTab();
  const hub = useApiHub();
  const toFilter = useElasticsearch();
  const castContentToSearchResult = useCastContentToSearchResult();
  const { isReady: settingsReady } = useSettings(true);
  const sortContent = useSortContent();
  const [{ requests }] = useApp();

  // This configures the shared storage between this list and any content tabs
  // that are opened.  Mainly used for navigation in the tab
  const [, setCurrentItems] = useLocalStorage('currentContent', {} as number[]);
  const [currentItemId, setCurrentItemId] = useLocalStorage('currentContentItemId', -1);

  const [isLoading, setIsLoading] = React.useState(false);
  const [isFilterLoading, setIsFilterLoading] = React.useState(true);
  const [selected, setSelected] = React.useState<IContentSearchResult[]>([]);
  const [focusedRowIndex, setFocusedRowIndex] = React.useState(id);
  const rowRefs = useRef<(HTMLDivElement | null)[]>([]);
  const [currentResultsPage, setCurrentResultsPage] = React.useState(defaultPage);
  const [totals, setTotals] = React.useState<ITotalsInfo>(defaultTotals);

  hub.useHubEffect(MessageTargetKey.ContentUpdated, (message) => {
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
    setCurrentItems(currentResultsPage.items.map((i) => i.id));
  }, [currentResultsPage.items, setCurrentItems]);

  // if the user navigates next/previous in another window change the highlighted row
  React.useEffect(() => {
    if (currentItemId !== -1) setFocusedRowIndex(currentItemId.toString());
  }, [currentItemId, setFocusedRowIndex]);

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
      const focusedRow = currentResultsPage.items.findIndex(
        (item) => item.id.toString() === focusedRowIndex,
      );
      if (rowRefs.current[focusedRow]) {
        rowRefs.current[focusedRow]?.scrollIntoView({ behavior: 'smooth', block: 'center' });
      }
    }
  }, [focusedRowIndex, currentResultsPage.items]);

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
          filter.pageIndex,
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

  const handleKeyDown = React.useCallback(
    (e: KeyboardEvent) => {
      if ((e.ctrlKey && e.key === 'ArrowUp') || (e.ctrlKey && e.key === 'ArrowDown')) {
        e.preventDefault();
        setFocusedRowIndex((prevIndex) => {
          let currentIndex = currentResultsPage.items.findIndex(
            (item) => item.id.toString() === prevIndex,
          );
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

          return currentResultsPage.items[newIndex].id.toString();
        });
      } else if (e.code === 'Enter' && focusedRowIndex !== null) {
        e.preventDefault();
        const focusedRow = currentResultsPage.items.find(
          (item) => item.id.toString() === focusedRowIndex,
        );
        if (focusedRow) {
          setSelected((selected) => {
            if (!selected.some((item) => item.id === focusedRow.id)) {
              return [...selected, focusedRow];
            }

            return selected.filter((item) => item.id !== focusedRow.id);
          });
        }
      }
    },
    [focusedRowIndex, currentResultsPage.items, setSelected],
  );

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

  React.useEffect(() => {
    window.addEventListener('keydown', handleKeyDown);
    return () => {
      window.removeEventListener('keydown', handleKeyDown);
    };
  }, [handleKeyDown]);

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

  const handleQuantityChange = React.useCallback(
    (quantity: number) => {
      if (filter.pageSize !== quantity) {
        const newFilter = {
          ...filter,
          pageSize: quantity,
        };
        storeFilterPaper(newFilter);
        replaceQueryParams(newFilter, { includeEmpty: false });
      }
    },
    [filter, storeFilterPaper],
  );

  const handlePageChange = React.useCallback(
    (page: number) => {
      if (filter.pageIndex !== page) {
        const newFilter = {
          ...filter,
          pageIndex: page,
        };
        storeFilterPaper(newFilter);
      }
    },
    [filter, storeFilterPaper],
  );

  const handleSortChange = React.useCallback(
    (column: IGridHeaderColumnProps, direction: SortDirection) => {
      if (column.name) {
        const sort =
          direction === SortDirection.None
            ? []
            : [{ id: column.name, desc: direction === SortDirection.Descending }];
        storeFilterPaper({ ...filter, sort });
      }
    },
    [filter, storeFilterPaper],
  );

  const handleContentClick = (id: number, event: React.MouseEvent<Element, MouseEvent>) => {
    setFocusedRowIndex(id.toString());
    setCurrentItemId(id);
    if (event.ctrlKey) navigate(id, '/contents', NavigateOptions.NewTab);
    else navigate(id);
  };

  return (
    <styled.Papers>
      <Col wrap="nowrap">
        <PaperToolbar onSearch={fetch} />
        <div className="paper-totals">
          <div>
            <div>
              Top Stor<span className="shortcut-key">i</span>es:
            </div>
            <div>{totals.topStories}</div>
          </div>
          <div>
            <div>
              Co<span className="shortcut-key">m</span>mentary:
            </div>
            <div>{totals.commentary}</div>
          </div>
          <div>
            <div>
              Feat<span className="shortcut-key">u</span>red Stories:
            </div>
            <div>{totals.featuredStories}</div>
          </div>
          <div>
            <div>
              Pu<span className="shortcut-key">b</span>lished:
            </div>
            <div>{totals.published}</div>
          </div>
        </div>
        <Row className="content-list">
          <Grid
            items={currentResultsPage.items}
            pageIndex={currentResultsPage.pageIndex}
            itemsPerPage={currentResultsPage.pageSize}
            totalItems={currentResultsPage.total}
            showPaging
            isLoading={
              isLoading || requests.some((r) => r.url === 'find-contents-with-elasticsearch')
            }
            onNavigatePage={async (page) => {
              handlePageChange(page);
            }}
            onQuantityChange={async (quantity) => {
              handleQuantityChange(quantity);
            }}
            onSortChange={async (column, direction) => {
              handleSortChange(column, direction);
            }}
            renderHeader={() => [
              { name: 'isChecked', label: '', size: '30px' },
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
              { name: 'otherSource', label: 'Source', sortable: true, size: '20%' },
              { name: 'mediaTypeId', label: 'Media Type', sortable: true, size: '20%' },
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
                        storeFilterPaperAdvanced(values);
                        await fetch({ ...filter, ...values });
                      }}
                    />
                  </Row>
                ),
                sortable: true,
              },
              { name: 'status', label: 'Use', sortable: true, size: '80px' },
            ]}
            renderColumns={(row: IContentSearchResult, rowIndex) => {
              const isSelected = selected.some((c) => row.id === c.id);
              const isFocused = focusedRowIndex === row.id.toString();

              const separator = row.page && row.section ? ':' : '';
              const pageSection = `${row.page}${separator}${row.section}`;

              return [
                {
                  key: row.id.toString(),
                  column: (
                    <div
                      key=""
                      ref={(el) => {
                        if (rowIndex) rowRefs.current[rowIndex] = el;
                      }}
                    >
                      <Checkbox
                        name={`chk-content-${row.id}`}
                        onChange={(e) =>
                          setSelected((selected) => {
                            if (e.target.checked) return [...selected, row];
                            else return selected.filter((c) => c.id !== row.id);
                          })
                        }
                        checked={selected.some((c) => c.id === row.id)}
                      />
                    </div>
                  ),
                  isSelected: isSelected,
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
                      onClick={(e) => handleContentClick(row.id, e)}
                    >
                      <CellEllipsis>{row.headline}</CellEllipsis>
                    </div>
                  ),
                  isSelected: isSelected,
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
                  isSelected: isSelected,
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
                  isSelected: isSelected,
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
                  isSelected: isSelected,
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
                  isSelected: isSelected,
                  isFocused: isFocused,
                },
              ];
            }}
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
