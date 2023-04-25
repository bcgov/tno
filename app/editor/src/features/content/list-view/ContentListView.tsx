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

import { ContentForm } from '../form';
import { ContentToolBar } from '../tool-bar';
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
  const { id: contentId = '' } = useParams();
  const [, { updateContent }] = useContentStore();
  const [
    { filter, filterAdvanced, content },
    { findContent, getContent, storeFilter, storeFilterAdvanced },
  ] = useContent();
  const navigate = useNavigate();
  const { combined, formType } = useCombinedView();
  var hub = useApiHub();
  const channel = useChannel<any>({
    onMessage: (ev) => {
      switch (ev.data.type) {
        case 'content':
          updateContent([ev.data.message]);
      }
    },
  });

  const [tab, setTab] = React.useState<Window | null>(null);
  const columns = getColumns((id) => {
    if (!tab || tab.closed) setTab(window.open(`/contents/${id}`, '_blank'));
    else {
      channel('fetch', id);
      tab.focus();
    }
  });

  const [contentType, setContentType] = React.useState(formType ?? ContentTypeName.Snippet);

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
        return new Page(data.page - 1, data.quantity, data?.items, data.total);
      } catch (error) {
        // TODO: Handle error
        throw error;
      }
    },
    [findContent],
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
    navigate(`/contents/combined/${row.original.id}${window.location.search}`);
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
              showSort={true}
              activeRowId={contentId}
              onPageChange={handleChangePage}
              onSortChange={handleChangeSort}
              onRowClick={handleRowClick}
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
