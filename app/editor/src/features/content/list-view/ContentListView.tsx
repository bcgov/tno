import React from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { HubMethodName, useApiHub, useApp, useChannel, useContent } from 'store/hooks';
import { useContentStore } from 'store/slices';
import {
  Col,
  ContentTypeName,
  FlexboxTable,
  IContentModel,
  ITableInternalRow,
  ITablePage,
  ITableSort,
  IWorkOrderModel,
  Page,
  Row,
  Show,
  useCombinedView,
  WorkOrderTypeName,
} from 'tno-core';

import { useTab } from '..';
import { ContentForm } from '../form';
import { ContentToolBar } from './components';
import { defaultPage, getColumns } from './constants';
import { IContentListAdvancedFilter, IContentListFilter } from './interfaces';
import * as styled from './styled';
import { makeFilter, queryToFilter, queryToFilterAdvanced } from './utils';

/**
 * ContentListView provides a way to list, search and select content for viewing and editing.
 * Also provides a combined view which splits the page into two columns.
 * @returns Component
 */
export const ContentListView: React.FC = () => {
  const [{ userInfo }] = useApp();
  const { id } = useParams();
  const [, { addContent, updateContent }] = useContentStore();
  const [
    { filter, filterAdvanced, content },
    { findContent, getContent, storeFilter, storeFilterAdvanced },
  ] = useContent();
  const navigate = useNavigate();
  const { combined, formType } = useCombinedView();
  var hub = useApiHub();
  const initTab = useTab();

  const channel = useChannel<any>({
    onMessage: (ev) => {
      switch (ev.data.type) {
        case 'content':
          if (content?.items.some((i) => i.id === ev.data.message.id))
            updateContent([ev.data.message]);
          else addContent([ev.data.message]);
          break;
        case 'page':
          channel('page', content);
          break;
        case 'load':
          setContentId(ev.data.message.id.toString());
          break;
      }
    },
  });

  const [contentId, setContentId] = React.useState(id);
  const [contentType, setContentType] = React.useState(formType ?? ContentTypeName.AudioVideo);

  const openTab = true; // TODO: Change to user preference and responsive in future.
  const columns = getColumns(openTab, initTab);

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
    (workOrder: IWorkOrderModel) => {
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

  React.useEffect(() => {
    return hub.listen(HubMethodName.WorkOrder, onWorkOrder);
  }, [onWorkOrder, hub]);

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
        const data = await findContent(
          makeFilter({
            ...filter,
          }),
        );
        const page = new Page(data.page - 1, data.quantity, data?.items, data.total);
        channel('page', page);
        return page;
      } catch (error) {
        // TODO: Handle error
        throw error;
      }
    },
    [channel, findContent],
  );

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
    (sort: ITableSort<IContentModel>[]) => {
      const sorts = sort.filter((s) => s.isSorted).map((s) => ({ id: s.id, desc: s.isSortedDesc }));
      storeFilter({ ...filter, sort: sorts });
    },
    [storeFilter, filter],
  );

  const handleRowClick = (row: ITableInternalRow<IContentModel>) => {
    setContentType(row.original.contentType);
    setContentId(row.original.id.toString());
    if (openTab) initTab(row.original.id);
    else navigate(`/contents/combined/${row.original.id}${window.location.search}`);
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
