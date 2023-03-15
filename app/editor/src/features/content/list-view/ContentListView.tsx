import {
  ContentTypeName,
  HubMethodName,
  IWorkOrderModel,
  useApiHub,
  useCombinedView,
  WorkOrderTypeName,
} from 'hooks';
import { IContentModel } from 'hooks/api-editor';
import React from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { SortingRule } from 'react-table';
import { Row as TRow } from 'react-table';
import { useApp, useContent } from 'store/hooks';
import { useContentStore } from 'store/slices';
import { Col, Page, PagedTable, Row, Show } from 'tno-core';

import { ContentForm } from '../form';
import { ContentToolBar } from '../tool-bar';
import { columns, defaultPage } from './constants';
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

  const [contentType, setContentType] = React.useState(formType ?? ContentTypeName.Snippet);
  const [loading, setLoading] = React.useState(false);

  const selectedRowIds = !!contentId
    ? ({ [contentId]: true } as Record<string, boolean>)
    : undefined;

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
        setLoading(true);
        const data = await findContent(
          makeFilter({
            ...filter,
          }),
        );
        return new Page(data.page - 1, data.quantity, data?.items, data.total);
      } catch (error) {
        // TODO: Handle error
        throw error;
      } finally {
        setLoading(false);
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
    (pi: number, ps?: number) => {
      if (filter.pageIndex !== pi || filter.pageSize !== ps)
        storeFilter({ ...filter, pageIndex: pi, pageSize: ps ?? filter.pageSize });
    },
    [filter, storeFilter],
  );

  const handleChangeSort = React.useCallback(
    (sortBy: SortingRule<IContentModel>[]) => {
      const sorts = sortBy.map((sb) => ({ id: sb.id, desc: sb.desc }));
      storeFilter({ ...filter, sort: sorts });
    },
    [storeFilter, filter],
  );

  const handleRowClick = (row: TRow<IContentModel>) => {
    setContentType(row.original.contentType);
    navigate(`/contents/combined/${row.original.id}`);
  };

  return (
    <styled.ContentListView>
      <Col wrap="nowrap">
        <ContentToolBar onSearch={fetch} />
        <Row className="top-pane">
          <Row className="content-list">
            <PagedTable
              columns={columns}
              page={page}
              isLoading={loading}
              sorting={{ sortBy: filter.sort }}
              getRowId={(content) => content.id.toString()}
              selectedRowIds={selectedRowIds}
              onRowClick={handleRowClick}
              onChangePage={handleChangePage}
              onChangeSort={handleChangeSort}
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
