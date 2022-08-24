import { FormPage } from 'components/form/formpage';
import { useCombinedView, useTooltips } from 'hooks';
import { IContentModel } from 'hooks/api-editor';
import React from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { SortingRule } from 'react-table';
import { useApp, useContent } from 'store/hooks';
import { Button, ButtonVariant, Col, Page, PagedTable, Row, Show } from 'tno-core';

import { FormPicker } from '../form';
import { ContentFilter } from '.';
import { columns, defaultPage } from './constants';
import { IContentListFilter } from './interfaces';
import * as styled from './styled';
import { makeFilter } from './utils';

export const ContentListView: React.FC = () => {
  const [{ userInfo }, { isUserReady }] = useApp();
  const { id } = useParams();
  const [{ filter, filterAdvanced, content }, { findContent, storeFilter }] = useContent();
  const navigate = useNavigate();
  const combined = useCombinedView();
  useTooltips();

  const [loading, setLoading] = React.useState(false);
  const [activeId, setActiveId] = React.useState<number>(parseInt(id ?? '0'));

  // Set the page for the grid table.
  const page = !!content
    ? new Page(content.page - 1, content.quantity, content?.items, content.total)
    : defaultPage;
  const userId = userInfo?.id ?? '';

  React.useEffect(() => {
    setActiveId(parseInt(id ?? '0'));
  }, [id]);

  React.useEffect(() => {
    if (userId !== 0 && filter.userId === '' && filter.userId !== userId) {
      storeFilter({ ...filter, userId });
    }
  }, [userId, filter, storeFilter]);

  const fetch = React.useCallback(
    async (filter: IContentListFilter) => {
      try {
        setLoading(true);
        const data = await findContent(
          makeFilter({
            ...filter,
            ...filterAdvanced,
          }),
        );
        const page = new Page(data.page - 1, data.quantity, data?.items, data.total);

        // setPage(page);
        return page;
      } catch (error) {
        // TODO: Handle error
        throw error;
      } finally {
        setLoading(false);
      }
    },
    [filterAdvanced, findContent],
  );

  React.useEffect(() => {
    // Only make a request if the user has been set.
    if (isUserReady() && filter.userId !== '') {
      fetch(filter);
    }
    // Only want to make a request when filter or sort change.
    // 'fetch' regrettably changes any time the advanced filter changes.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [filter, isUserReady()]);

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
      const same = sorts.every(
        (val, i) => val.id === filter.sort[i]?.id && val.desc === filter.sort[i]?.desc,
      );
      if (!same) {
        storeFilter({ ...filter, sort: sorts });
      }
    },
    [storeFilter, filter],
  );

  const handleRowClick = (content: IContentModel) => {
    setActiveId(content.id);
    navigate(`/contents/combined/${content.id}`);
  };

  return (
    <styled.ContentListView>
      <FormPage>
        <Row wrap="nowrap">
          <Col className="left-pane">
            <ContentFilter search={fetch} />
            <Row className="content-list">
              <PagedTable
                columns={columns}
                page={page}
                isLoading={loading}
                sorting={{ sortBy: filter.sort }}
                onRowClick={(row) => handleRowClick(row.original)}
                activeId={activeId}
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
              <div>Send to</div>
              <Button
                name="create"
                variant={ButtonVariant.secondary}
                disabled
                tooltip="Under Construction"
              >
                Front Pages
              </Button>
              <Button
                name="create"
                variant={ButtonVariant.secondary}
                disabled
                tooltip="Under Construction"
              >
                Top Stories
              </Button>
              <Button
                name="create"
                variant={ButtonVariant.secondary}
                disabled
                tooltip="Under Construction"
              >
                Commentary
              </Button>
            </Row>
          </Col>
          <Show visible={combined}>
            <Col className="right-pane">
              <FormPicker />
            </Col>
          </Show>
        </Row>
      </FormPage>
    </styled.ContentListView>
  );
};
