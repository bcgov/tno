import { ContentTypeName, useCombinedView, useTooltips } from 'hooks';
import { IContentModel } from 'hooks/api-editor';
import React from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { SortingRule } from 'react-table';
import { Row as TRow } from 'react-table';
import { useApp, useContent } from 'store/hooks';
import { Button, ButtonVariant, Col, Page, PagedTable, Row, Show } from 'tno-core';

import { ContentForm } from '../form';
import { ContentFilter } from '.';
import { columns, defaultPage } from './constants';
import { IContentListAdvancedFilter, IContentListFilter } from './interfaces';
import * as styled from './styled';
import { makeFilter } from './utils';

/**
 * ContentListView provides a way to list, search and select content for viewing and editing.
 * Also provides a combined view which splits the page into two columns.
 * @returns Component
 */
export const ContentListView: React.FC = () => {
  const [{ userInfo }] = useApp();
  const { id: contentId = '' } = useParams();
  const [{ filter, filterAdvanced, content }, { findContent, storeFilter }] = useContent();
  const navigate = useNavigate();
  const { combined, formType } = useCombinedView();
  useTooltips();

  const [contentType, setContentType] = React.useState(formType ?? ContentTypeName.Snippet);
  const [loading, setLoading] = React.useState(false);

  const page = React.useMemo(
    () =>
      !!content
        ? new Page(content.page - 1, content.quantity, content?.items, content.total)
        : defaultPage,
    [content],
  );
  const userId = userInfo?.id ?? '';
  const isReady = !!userId && filter.userId !== '';

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
    // Required because the first time this page is loaded directly the user has not been set.
    // Don't make a request until the user has been set.
    if (userId !== '' && filter.userId === '') {
      storeFilter({ ...filter, userId });
    }
  }, [userId, filter, storeFilter]);

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

  const hideColumns = (combined: boolean, contentType?: ContentTypeName) => {
    const hiddenColumns = [];

    if (combined) {
      hiddenColumns.push('ownerId');
      hiddenColumns.push('status');
    }

    if (contentType !== ContentTypeName.PrintContent) {
      hiddenColumns.push('page');
    }

    return hiddenColumns;
  };

  return (
    <styled.ContentListView maxWidth={combined ? 'fit-content' : ''}>
      <Row wrap="nowrap">
        <Col className="left-pane">
          <ContentFilter onSearch={fetch} />
          <Row className="content-list">
            <PagedTable
              columns={columns}
              hiddenColumns={hideColumns(combined, filter.contentType)}
              page={page}
              isLoading={loading}
              sorting={{ sortBy: filter.sort }}
              getRowId={(content) => content.id.toString()}
              selectedRowIds={{ [contentId]: true }}
              onRowClick={handleRowClick}
              onChangePage={handleChangePage}
              onChangeSort={handleChangeSort}
            />
          </Row>
          <Row className="content-actions">
            <Button
              name="create"
              onClick={() => navigate('/snippets/0')}
              variant={ButtonVariant.secondary}
            >
              Create Snippet
            </Button>
            <Button
              name="create"
              onClick={() => navigate('/papers/0')}
              variant={ButtonVariant.secondary}
            >
              Create Print Content
            </Button>
            <Button
              name="create"
              onClick={() => navigate('/images/0')}
              variant={ButtonVariant.secondary}
            >
              Create Image
            </Button>
          </Row>
        </Col>
        <Show visible={combined}>
          <Col className="right-pane">
            <ContentForm contentType={contentType} />
          </Col>
        </Show>
      </Row>
    </styled.ContentListView>
  );
};
