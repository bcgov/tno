import { useFormikContext } from 'formik';
import moment from 'moment';
import React from 'react';
import { Row as rtRow, SortingRule } from 'react-table';
import { toast } from 'react-toastify';
import { useApp, useLookup } from 'store/hooks';
import { useContentReferences } from 'store/hooks/admin';
import {
  FieldSize,
  getSortableOptions,
  IconButton,
  IContentReferenceFilter,
  IContentReferenceModel,
  IIngestModel,
  IPage,
  OptionItem,
  Page,
  PagedTable,
  Row,
  Select,
  SelectDate,
  Text,
} from 'tno-core';

import {
  contentReferenceColumns,
  defaultContentReferenceFilter,
  defaultContentReferencePage,
} from './constants';
import { IContentReferenceListFilter } from './interfaces';
import * as styled from './styled';

export interface IContentReferenceListProps {}

const ContentReferenceList: React.FC<IContentReferenceListProps> = (props) => {
  const [{ requests }] = useApp();
  const [, { findContentReferences, findContentIds }] = useContentReferences();
  const { values } = useFormikContext<IIngestModel>();
  const [{ sources }] = useLookup();

  const [filter, setFilter] = React.useState<IContentReferenceListFilter>({
    ...defaultContentReferenceFilter,
    sources: values.source?.code ? [values.source.code] : [],
    mediaTypeIds: [values.mediaTypeId],
  });
  const [page, setPage] = React.useState<IPage<IContentReferenceModel>>(
    defaultContentReferencePage,
  );

  // Extract possible sources for this ingest from the configuration, or list all options.
  const configSources = [values.source?.code]
    .concat(values.configuration?.sources?.split('&').map((s: string) => s.split('=')[1]) ?? [])
    .filter((s) => s !== undefined) as string[];
  const ingestSources = configSources.length
    ? configSources
        .map((code) => sources.find((source) => source.code === code)!)
        .filter((source) => !!source)
    : sources;
  const sourceOptions = getSortableOptions(
    ingestSources,
    values.sourceId,
    [],
    (i) => new OptionItem(`(${i.code}) ${i.name}`, i.id),
  );

  const fetch = React.useCallback(
    async (filter: IContentReferenceListFilter) => {
      try {
        const query: IContentReferenceFilter = {
          page: filter.pageIndex + 1,
          quantity: filter.pageSize,
          sources: filter.sources,
          mediaTypeIds: filter.mediaTypeIds,
          uid: filter.uid,
          status: filter.status ? filter.status : undefined,
          publishedOn: filter.publishedOn ? moment(filter.publishedOn).toISOString() : undefined,
          publishedStartOn: filter.publishedStartOn
            ? moment(filter.publishedStartOn).toISOString()
            : undefined,
          publishedEndOn: filter.publishedEndOn
            ? moment(filter.publishedEndOn).toISOString()
            : undefined,
          sort: filter.sort.length ? filter.sort.map((s) => `${s.id}${s.desc ? ' desc' : ''}`) : [],
        };
        const result = await findContentReferences(query);
        setPage(new Page(result.page - 1, result.quantity, result.items, result.total));
        return result;
      } catch {}
    },
    [findContentReferences],
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

  const handleRowClick = React.useCallback(
    async (row: rtRow<IContentReferenceModel>) => {
      const ids = await findContentIds(row.original.uid);
      if (ids.length) {
        window.open(`/contents/${ids[0]}`, '_blank');
      } else {
        toast.error('No content found, the uid may have been changed.');
      }
    },
    [findContentIds],
  );

  return (
    <styled.ContentReferenceList>
      <h2>{values.name}</h2>
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
        <Select
          label="Source"
          name="sources"
          options={sourceOptions}
          onChange={(newValue: any) => {
            // Use the source.code to set the Kafka topic.
            const source = sources.find((s) => s.id === newValue.value);
            setFilter({ ...filter, sources: source ? [source.code] : [] });
          }}
          clearValue={''}
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
        sorting={{ sortBy: filter.sort }}
        onChangePage={handleChangePage}
        onChangeSort={handleChangeSort}
        onRowClick={handleRowClick}
      />
    </styled.ContentReferenceList>
  );
};

export default ContentReferenceList;
