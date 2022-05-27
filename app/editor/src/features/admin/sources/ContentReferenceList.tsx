import { FieldSize, IconButton, SelectDate, Text } from 'components/form';
import { useFormikContext } from 'formik';
import {
  IContentReferenceFilter,
  IContentReferenceModel,
  IDataSourceModel,
} from 'hooks/api-editor';
import moment from 'moment';
import React from 'react';
import { SortingRule } from 'react-table';
import { useApp } from 'store/hooks';
import { useContentReferences } from 'store/hooks/admin';
import { Page, PagedTable, Row } from 'tno-core';

import {
  contentReferenceColumns,
  defaultContentReferenceFilter,
  defaultContentReferencePage,
} from './constants';
import { IContentReferenceListFilter } from './interfaces';
import * as styled from './styled';

export interface IContentReferenceListProps {}

export const ContentReferenceList: React.FC<IContentReferenceListProps> = (props) => {
  const [{ requests }] = useApp();
  const [, api] = useContentReferences();
  const { values } = useFormikContext<IDataSourceModel>();

  const [filter, setFilter] = React.useState<IContentReferenceListFilter>({
    ...defaultContentReferenceFilter,
    source: values.code,
  });
  const [page, setPage] = React.useState<Page<IContentReferenceModel>>(defaultContentReferencePage);

  const fetch = React.useCallback(
    async (filter: IContentReferenceListFilter) => {
      try {
        const query: IContentReferenceFilter = {
          page: filter.pageIndex + 1,
          quantity: filter.pageSize,
          source: filter.source,
          uid: filter.uid,
          workflowStatus: filter.workflowStatus ? filter.workflowStatus : undefined,
          publishedOn: filter.publishedOn ? moment(filter.publishedOn).toISOString() : undefined,
          publishedStartOn: filter.publishedStartOn
            ? moment(filter.publishedStartOn).toISOString()
            : undefined,
          publishedEndOn: filter.publishedEndOn
            ? moment(filter.publishedEndOn).toISOString()
            : undefined,
          sort: filter.sort.length ? filter.sort.map((s) => `${s.id}${s.desc ? ' desc' : ''}`) : [],
        };
        const result = await api.findContentReferences(query);
        setPage(new Page(result.page - 1, result.quantity, result.items, result.total));
        return result;
      } catch {}
    },
    [api],
  );

  React.useEffect(() => {
    fetch(filter);
  }, [fetch, filter]);

  const handleChangePage = React.useCallback(
    (pi: number, ps?: number) => {
      if (filter.pageIndex !== pi || filter.pageSize !== ps)
        setFilter({ ...filter, pageIndex: pi, pageSize: ps ?? filter.pageSize });
    },
    [filter],
  );

  const handleChangeSort = React.useCallback(
    (sortBy: SortingRule<IContentReferenceModel>[]) => {
      const sorts = sortBy.map((sb) => ({ id: sb.id, desc: sb.desc }));
      const same = sorts.every(
        (val, i) => val.id === filter.sort[i]?.id && val.desc === filter.sort[i]?.desc,
      );
      if (!same) {
        setFilter({ ...filter, sort: sorts });
      }
    },
    [filter],
  );

  return (
    <styled.ContentReferenceList>
      <p>
        Content identified by this data source. This table only represents a reference to the
        content, it is used for debugging.
      </p>
      <Row alignContent="flex-end" alignItems="flex-end" className="filter">
        <Text
          name="uid"
          label="UID"
          onBlur={(e) => {
            setFilter({ ...filter, uid: e.target.value });
          }}
        />
        <SelectDate
          name="startDate"
          label="Published On Start"
          placeholderText="YYYY MM DD"
          selected={!!filter.publishedStartOn ? new Date(filter.publishedStartOn) : undefined}
          width={FieldSize.Small}
          onChange={(date) =>
            setFilter({
              ...filter,
              publishedStartOn: !!date ? date.toString() : '',
            })
          }
        />
        <SelectDate
          name="endDate"
          label="Published On End"
          placeholderText="YYYY MM DD"
          selected={!!filter.publishedEndOn ? new Date(filter.publishedEndOn) : undefined}
          width={FieldSize.Small}
          onChange={(date) => {
            date?.setHours(23, 59, 59);
            setFilter({
              ...filter,
              publishedEndOn: !!date ? date.toString() : '',
            });
          }}
        />
        <IconButton
          iconType="search"
          onClick={() => {
            fetch(filter);
          }}
        />
        <IconButton
          iconType="reset"
          onClick={() => {
            setFilter({ ...filter, uid: '', publishedStartOn: '', publishedEndOn: '' });
          }}
        />
      </Row>
      <PagedTable
        columns={contentReferenceColumns}
        page={page}
        isLoading={!!requests.length}
        sortBy={filter.sort}
        onChangePage={handleChangePage}
        onChangeSort={handleChangeSort}
      />
    </styled.ContentReferenceList>
  );
};
