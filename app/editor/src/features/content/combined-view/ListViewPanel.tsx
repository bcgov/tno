import { IContentModel } from 'hooks';
import React from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { SortingRule } from 'react-table';
import { useApp, useContent } from 'store/hooks';
import { Button, Page, PagedTable, Row } from 'tno-core';

import { ContentFilter } from '../list-view';
import { condensedColumns, defaultPage } from '../list-view/constants';
import { IContentListFilter } from '../list-view/interfaces';
import { makeFilter } from '../list-view/utils';
import * as styled from './styled';

export interface IListViewPanel {
  /** boolean value to track when content is updated */
  updated: boolean;
  setUpdated: (value: boolean) => void;
}

export const ListViewPanel: React.FC<IListViewPanel> = ({ updated, setUpdated }) => {
  const navigate = useNavigate();
  const [{ filter, content, filterAdvanced }, { storeFilter, findContent }] = useContent();
  const { id } = useParams();
  const fetch = React.useCallback(
    async (filter: IContentListFilter) => {
      try {
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

  const page = !!content
    ? new Page(content.page - 1, content.quantity, content?.items, content.total)
    : defaultPage;

  const [{ requests }] = useApp();

  return (
    <styled.ListViewPanel>
      <Row className="filter-area">
        <ContentFilter updated={updated} setUpdated={setUpdated} search={fetch} />
      </Row>
      <PagedTable
        columns={condensedColumns}
        activeId={Number(id)}
        page={page}
        isLoading={!!requests.length}
        sorting={{ sortBy: filter.sort }}
        onRowClick={(row) => navigate(`/contents/combined/${row.original.id}`)}
        onChangePage={handleChangePage}
        onChangeSort={handleChangeSort}
      />
      <Button className="create-snippet" name="create" onClick={() => navigate('/contents/0')}>
        Create Snippet
      </Button>
    </styled.ListViewPanel>
  );
};
