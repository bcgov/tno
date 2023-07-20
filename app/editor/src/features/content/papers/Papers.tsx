import React from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { toast } from 'react-toastify';
import { HubMethodName, useApiHub, useChannel, useContent } from 'store/hooks';
import { useContentStore } from 'store/slices';
import {
  Col,
  ContentTypeName,
  FlexboxTable,
  IContentMessageModel,
  IContentModel,
  ITableInternalCell,
  ITableInternalRow,
  ITablePage,
  ITableSort,
  Page,
  replaceQueryParams,
  Row,
  Show,
  useCombinedView,
} from 'tno-core';

import { useTab } from '..';
import { ContentForm } from '../form';
import { defaultPage } from '../list-view/constants';
import { IContentListAdvancedFilter } from '../list-view/interfaces';
import { ReportActions } from './components';
import { getColumns } from './constants';
import { IPaperFilter } from './interfaces';
import { PaperFilter } from './PaperFilter';
import * as styled from './styled';
import { makeFilter } from './utils';

export interface IPapersProps extends React.HTMLAttributes<HTMLDivElement> {}

/**
 * Provides a list view of print content to help select stories for the papers.
 * @param props Component props.
 * @returns Component.
 */
export const Papers: React.FC<IPapersProps> = (props) => {
  const { id } = useParams();
  const navigate = useNavigate();
  const { combined, formType } = useCombinedView();
  const [
    { filterPaper: filter, filterPaperAdvanced: filterAdvanced, content },
    { findContent, storeFilterPaper, updateContent: updateStatus, getContent },
  ] = useContent();
  const [, { addContent, updateContent }] = useContentStore();
  const initTab = useTab();
  var hub = useApiHub();

  const [contentId, setContentId] = React.useState(id);
  const [contentType, setContentType] = React.useState(formType ?? ContentTypeName.AudioVideo);

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

  const [isLoading, setIsLoading] = React.useState(false);
  const [selected, setSelected] = React.useState<IContentModel[]>([]);

  const onContentUpdated = React.useCallback(
    async (message: IContentMessageModel) => {
      const item = await getContent(message.id);
      if (!!item) {
        if (content?.items.some((i) => i.id === item.id)) updateContent([item]);
        else addContent([item]);
      }
    },
    [addContent, content?.items, getContent, updateContent],
  );

  React.useEffect(() => {
    return hub.listen(HubMethodName.Content, onContentUpdated);
  }, [onContentUpdated, hub]);

  const handleClickUse = React.useCallback(
    (content: IContentModel) => {
      updateStatus(content)
        .then((response) => {
          updateContent([response]);
          toast.success(
            `"${content.headline}" has been updated.  A request has been sent to update the index.`,
          );
        })
        .catch((error) => {});
    },
    [updateContent, updateStatus],
  );

  const openTab = true; // TODO: Change to user preference and responsive in future.
  const columns = getColumns(openTab, initTab, handleClickUse);

  const page = React.useMemo(
    () =>
      !!content
        ? new Page(content.page - 1, content.quantity, content?.items, content.total)
        : defaultPage,
    [content],
  );

  const fetch = React.useCallback(
    async (filter: IPaperFilter & Partial<IContentListAdvancedFilter>) => {
      try {
        if (!isLoading) {
          setIsLoading(true);
          const data = await findContent(
            makeFilter({
              ...filter,
            }),
          );
          const page = new Page(data.page - 1, data.quantity, data?.items, data.total);
          channel('page', page);
          return page;
        }
      } catch (error) {
        // TODO: Handle error
        throw error;
      } finally {
        setIsLoading(false);
      }
    },
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [channel, findContent],
  );

  React.useEffect(() => {
    fetch({ ...filter, ...filterAdvanced });
    // Do not want to fetch when the advanced filter changes.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [filter, fetch]);

  const handleRowClick = (
    cell: ITableInternalCell<IContentModel>,
    row: ITableInternalRow<IContentModel>,
  ) => {
    if (cell.index > 0 && cell.index !== 6) {
      setContentType(row.original.contentType);
      setContentId(row.original.id.toString());
      if (openTab) initTab(row.original.id);
      else navigate(`/papers/combined/${row.original.id}${window.location.search}`);
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
    (sort: ITableSort<IContentModel>[]) => {
      const sorts = sort.filter((s) => s.isSorted).map((s) => ({ id: s.id, desc: s.isSortedDesc }));
      storeFilterPaper({ ...filter, sort: sorts });
    },
    [filter, storeFilterPaper],
  );

  const handleSelectedRowsChanged = (row: ITableInternalRow<IContentModel>) => {
    if (row.isSelected) {
      setSelected(row.table.rows.filter((r) => r.isSelected).map((r) => r.original));
    } else {
      setSelected((selected) => selected.filter((r) => r.id !== row.original.id));
    }
  };

  return (
    <styled.Papers>
      <Col wrap="nowrap">
        <PaperFilter onSearch={fetch} />
        <Row className="content-list">
          <FlexboxTable
            rowId="id"
            columns={columns}
            data={page.items}
            isMulti={true}
            manualPaging={true}
            pageIndex={filter.pageIndex}
            pageSize={filter.pageSize}
            pageCount={page.pageCount}
            showSort={true}
            activeRowId={contentId}
            isLoading={isLoading}
            onPageChange={handleChangePage}
            onSortChange={handleChangeSort}
            onCellClick={handleRowClick}
            onSelectedChanged={handleSelectedRowsChanged}
          />
        </Row>
        <ReportActions setLoading={setIsLoading} selected={selected} filter={filter} />
        <Show visible={combined}>
          <hr />
          <Row className="bottom-pane" id="bottom-pane">
            <ContentForm
              contentType={contentType}
              scrollToContent={false}
              combinedPath="/papers/combined"
            />
          </Row>
        </Show>
      </Col>
    </styled.Papers>
  );
};
