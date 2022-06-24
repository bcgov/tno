import { IContentModel } from 'hooks';
import React from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { SortingRule } from 'react-table';
import { useApp, useContent } from 'store/hooks';
import { Page, PagedTable } from 'tno-core';

import { condensedColumns, defaultPage } from '../list-view/constants';
import * as styled from './styled';

export interface IListViewPanel {}

export const ListViewPanel: React.FC<IListViewPanel> = () => {
  const navigate = useNavigate();
  const [{ filter, content }, { storeFilter }] = useContent();
  const { id } = useParams();
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
    </styled.ListViewPanel>
  );
};
