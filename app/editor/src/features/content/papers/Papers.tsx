import { NavigateOptions, useTab } from 'components/tab-control';
import React, { lazy } from 'react';
import { useParams } from 'react-router-dom';
import { toast } from 'react-toastify';
import { useApiHub, useApp, useContent } from 'store/hooks';
import { IContentSearchResult, useContentStore } from 'store/slices';
import {
  Col,
  ContentTypeName,
  FlexboxTable,
  IContentMessageModel,
  ITableInternalCell,
  ITableInternalRow,
  ITablePage,
  ITableSort,
  MessageTargetName,
  Page,
  replaceQueryParams,
  Row,
  Show,
  useCombinedView,
} from 'tno-core';

import { useElasticsearch } from '../hooks';
import { IContentListAdvancedFilter, IContentListFilter } from '../interfaces';
import { defaultPage } from '../list-view/constants';
import { ReportActions } from './components';
import { useColumns } from './hooks';
import { PaperFilter } from './PaperFilter';
import * as styled from './styled';
const ContentForm = lazy(() => import('../form/ContentForm'));

export interface IPapersProps extends React.HTMLAttributes<HTMLDivElement> {}

/**
 * Provides a list view of print content to help select stories for the papers.
 * @param props Component props.
 * @returns Component.
 */
const Papers: React.FC<IPapersProps> = (props) => {
  const [{ userInfo }] = useApp();
  const { id } = useParams();
  const { combined, formType } = useCombinedView();
  const [
    { filterPaper: filter, filterPaperAdvanced: filterAdvanced, searchResults },
    { findContentWithElasticsearch, storeFilterPaper, updateContent: updateStatus, getContent },
  ] = useContent();
  const [, { addContent, updateContent }] = useContentStore();
  const { navigate } = useTab();
  const hub = useApiHub();
  const toFilter = useElasticsearch();

  const [contentId, setContentId] = React.useState(id);
  const [contentType, setContentType] = React.useState(formType ?? ContentTypeName.AudioVideo);

  const [isLoading, setIsLoading] = React.useState(false);
  const [selected, setSelected] = React.useState<IContentSearchResult[]>([]);

  const userId = userInfo?.id ?? '';

  const onContentAdded = React.useCallback(
    async (message: IContentMessageModel) => {
      if (message.ownerId === userId) {
        try {
          const result = await getContent(message.id);
          if (!!result) addContent([result]);
        } catch {}
      }
    },
    [userId, getContent, addContent],
  );
  hub.useHubEffect(MessageTargetName.ContentAdded, onContentAdded);

  const onContentUpdated = React.useCallback(
    async (message: IContentMessageModel) => {
      if (searchResults?.items.some((c) => c.id === message.id)) {
        try {
          const item = await getContent(message.id);
          if (!!item) updateContent([item]);
        } catch {}
      }
    },
    [searchResults?.items, getContent, updateContent],
  );
  hub.useHubEffect(MessageTargetName.ContentUpdated, onContentUpdated);

  const handleClickUse = React.useCallback(
    (content: IContentSearchResult) => {
      getContent(content.id)
        .then((response) => {
          if (response) {
            return updateStatus({ ...response, status: content.status }).then((content) => {
              updateContent([content]);
              toast.success(
                `"${content.headline}" has been updated.  A request has been sent to update the index.`,
              );
            });
          } else Promise.reject('Content does not exist');
        })
        .catch((error) => {});
    },
    [getContent, updateContent, updateStatus],
  );

  const page = React.useMemo(
    () =>
      !!searchResults
        ? new Page(
            searchResults.page - 1,
            filter.pageSize,
            searchResults?.items,
            searchResults.total,
          )
        : defaultPage,
    [filter.pageSize, searchResults],
  );

  const fetch = React.useCallback(
    async (filter: IContentListFilter & Partial<IContentListAdvancedFilter>) => {
      try {
        setIsLoading(true);
        const results = await findContentWithElasticsearch(toFilter(filter), true);
        const data = results.hits.hits.filter((h) => !!h._source).map((h) => h._source!);
        const page = new Page(
          filter.pageIndex,
          filter.pageSize,
          data,
          results.hits.total as number,
        );
        return page;
      } catch (error) {
        // TODO: Handle error
        throw error;
      } finally {
        setIsLoading(false);
      }
    },
    [findContentWithElasticsearch, toFilter],
  );

  const columns = useColumns({ fetch, onClickUse: handleClickUse });

  React.useEffect(() => {
    fetch({ ...filter, ...filterAdvanced });
    // Do not want to fetch when the advanced filter changes.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [filter]);

  const handleRowClick = (
    cell: ITableInternalCell<IContentSearchResult>,
    row: ITableInternalRow<IContentSearchResult>,
    event: React.MouseEvent<Element, MouseEvent>,
  ) => {
    if (cell.index > 0 && cell.index !== 5) {
      setContentType(row.original.contentType);
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
            totalItems={page.total}
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

export default Papers;
