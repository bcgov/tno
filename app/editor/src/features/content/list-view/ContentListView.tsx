import { FormPage } from 'components/form/formpage';
import { IContentModel } from 'hooks/api-editor';
import React from 'react';
import { useNavigate } from 'react-router-dom';
import { SortingRule } from 'react-table';
import ReactTooltip from 'react-tooltip';
import { useApp, useContent } from 'store/hooks';
import { Button, ButtonVariant, Page, PagedTable } from 'tno-core';

import { ContentFilter } from '.';
import { columns, defaultPage } from './constants';
import { IContentListFilter } from './interfaces';
import * as styled from './styled';
import { makeFilter } from './utils';

export const ContentListView: React.FC = () => {
  const [{ userInfo, requests }, { isUserReady }] = useApp();
  const userId = userInfo?.id ?? '';
  const [{ filter, filterAdvanced, content }, { findContent, storeFilter }] = useContent();
  const navigate = useNavigate();

  // Set the page for the grid table.
  const page = !!content
    ? new Page(content.page - 1, content.quantity, content?.items, content.total)
    : defaultPage;

  React.useEffect(() => {
    ReactTooltip.rebuild();
  });

  React.useEffect(() => {
    if (userId !== 0 && filter.userId === '' && filter.userId !== userId) {
      storeFilter({ ...filter, userId });
    }
  }, [userId, filter, storeFilter]);

  const fetch = React.useCallback(
    async (filter: IContentListFilter) => {
      try {
        alert('try');
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

  return (
    <styled.ContentListView>
      <FormPage>
        <ContentFilter search={fetch} />
        <div className="content-list">
          <PagedTable
            columns={columns}
            page={page}
            isLoading={!!requests.length}
            sorting={{ sortBy: filter.sort }}
            onRowClick={(row) => navigate(`/contents/combined/${row.original.id}`)}
            onChangePage={handleChangePage}
            onChangeSort={handleChangeSort}
          />
        </div>
        <div className="content-actions">
          <Button name="create" onClick={() => navigate('/contents/0')}>
            Create Snippet
          </Button>
          <div style={{ marginTop: '2%' }} className="addition-actions">
            <Button
              name="create"
              variant={ButtonVariant.secondary}
              disabled
              tooltip="Under Construction"
            >
              Send Lois Front Pages
            </Button>
            <Button
              name="create"
              variant={ButtonVariant.secondary}
              disabled
              tooltip="Under Construction"
            >
              Send Top Stories
            </Button>
            <Button
              name="create"
              variant={ButtonVariant.secondary}
              disabled
              tooltip="Under Construction"
            >
              Send Lois to Commentary
            </Button>
          </div>
        </div>
      </FormPage>
    </styled.ContentListView>
  );
};
