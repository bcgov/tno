import { NavigateOptions, useTab } from 'components/tab-control';
import React, { lazy } from 'react';
import { useParams } from 'react-router-dom';
import { useApiHub, useApp, useContent } from 'store/hooks';
import { IContentSearchResult, useContentStore } from 'store/slices';
import {
  Col,
  ContentTypeName,
  FlexboxTable,
  IContentMessageModel,
  ITableInternalRow,
  ITablePage,
  ITableSort,
  IWorkOrderMessageModel,
  MessageTargetName,
  Page,
  Row,
  Show,
  useCombinedView,
  WorkOrderTypeName,
} from 'tno-core';

import { ContentToolBar } from './components';
import { defaultPage } from './constants';
import { useColumns } from './hooks';
import { IContentListAdvancedFilter, IContentListFilter } from './interfaces';
import * as styled from './styled';
import { makeFilter, queryToFilter, queryToFilterAdvanced } from './utils';

const ContentForm = lazy(() => import('../form/ContentForm'));

/**
 * ContentListView provides a way to list, search and select content for viewing and editing.
 * Also provides a combined view which splits the page into two columns.
 * @returns Component
 */
const ContentListView: React.FC = () => {
  const [{ userInfo }] = useApp();
  const { id } = useParams();
  const [, { updateContent }] = useContentStore();
  const [
    { filter, filterAdvanced, content },
    { findContent, getContent, storeFilter, storeFilterAdvanced },
  ] = useContent();
  const { combined, formType } = useCombinedView();
  var hub = useApiHub();
  const { navigate } = useTab({ showNav: false });

  const [contentId, setContentId] = React.useState(id);
  const [contentType, setContentType] = React.useState(formType ?? ContentTypeName.AudioVideo);
  const [isLoading, setIsLoading] = React.useState(false);

  React.useEffect(() => {
    // Extract query string values and place them into redux store.
    if (!!window.location.search) {
      storeFilter(queryToFilter(filter, window.location.search));
      storeFilterAdvanced(queryToFilterAdvanced(filterAdvanced, window.location.search));
    }
    // Only want this to run on the first load.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const page = React.useMemo(
    () =>
      !!content
        ? new Page(content.page - 1, content.quantity, content?.items, content.total)
        : defaultPage,
    [content],
  );
  const userId = userInfo?.id ?? '';
  const isReady = !!userId && filter.userId !== '';

  const onWorkOrder = React.useCallback(
    (workOrder: IWorkOrderMessageModel) => {
      if (
        [WorkOrderTypeName.Transcription, WorkOrderTypeName.NaturalLanguageProcess].includes(
          workOrder.workType,
        )
      ) {
        if (!!workOrder.configuration.contentId) {
          getContent(workOrder.configuration.contentId ?? 0).then((content) => {
            if (!!content) updateContent([content]);
          });
        }
      }
    },
    [getContent, updateContent],
  );

  hub.useHubEffect(MessageTargetName.WorkOrder, onWorkOrder);

  const onContentUpdated = React.useCallback(
    async (message: IContentMessageModel) => {
      // Only update if the current page includes the updated content.
      if (content?.items.some((c) => c.id === message.id)) {
        try {
          const result = await getContent(message.id);
          if (!!result) updateContent([result]);
        } catch {}
      }
    },
    [content?.items, getContent, updateContent],
  );

  hub.useHubEffect(MessageTargetName.ContentUpdated, onContentUpdated);

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
          const data = await findContent(
            makeFilter({
              ...filter,
            }),
          );
          const page = new Page(data.page - 1, data.quantity, data?.items, data.total);
          return page;
        }
      } catch {
      } finally {
        setIsLoading(false);
      }
    },
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [findContent],
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
      if (filter.pageIndex !== page.pageIndex || filter.pageSize !== page.pageSize)
        storeFilter({
          ...filter,
          pageIndex: page.pageIndex,
          pageSize: page.pageSize ?? filter.pageSize,
        });
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
      const index = !!contentId ? page?.items.findIndex((c: any) => c.id === +contentId) ?? -1 : -1;
      const newIndex = event.key === 'ArrowUp' ? index - 1 : index + 1;
      const newContent = page.items[newIndex];
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
              data={page.items}
              manualPaging={true}
              pageIndex={filter.pageIndex}
              pageSize={filter.pageSize}
              pageCount={page.pageCount}
              totalItems={page.total}
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
