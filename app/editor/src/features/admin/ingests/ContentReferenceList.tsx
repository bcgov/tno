import { useFormikContext } from 'formik';
import moment from 'moment';
import React from 'react';
import { type SortingRule } from 'react-table';
import { useApp, useLookup } from 'store/hooks';
import { useContentReferences } from 'store/hooks/admin';
import {
  FieldSize,
  IconButton,
  type IIngestModel,
  type IPage,
  OptionItem,
  Page,
  PagedTable,
  Select,
  SelectDate,
  Text,
  WorkflowStatusName,
} from 'tno-core';

import {
  contentReferenceColumns,
  defaultContentReferenceFilter,
  defaultContentReferencePage,
} from './constants';
import {
  type IContentReferenceListFilter,
  type IIngestContentReference,
  type IIngestContentReferenceFilter,
  type ISyndicationFeed,
} from './interfaces';
import * as styled from './styled';

const statusOptions = [
  new OptionItem('All statuses', ''),
  ...Object.values(WorkflowStatusName).map((status) => new OptionItem(status, status)),
];

export interface IContentReferenceListProps {}

/**
 * The content references an ingest discovered on a day, to help debug the ingest.
 * Each reference links to the content imported for it.
 */
const ContentReferenceList: React.FC<IContentReferenceListProps> = (props) => {
  const [{ requests }] = useApp();
  const [, { findContentReferences }] = useContentReferences();
  const { values } = useFormikContext<IIngestModel>();
  const [{ sources }] = useLookup();

  const [filter, setFilter] = React.useState<IContentReferenceListFilter>({
    ...defaultContentReferenceFilter,
    discoveredOn: new Date(),
  });
  const [page, setPage] = React.useState<IPage<IIngestContentReference>>(
    defaultContentReferencePage,
  );

  // The source codes this ingest assigns to content: its source, the source mapping, the default
  // source and the feed sources.
  const ingestSources = React.useMemo(
    () =>
      [values.source?.code, values.configuration?.defaultSource]
        .concat(
          values.configuration?.sources?.split('&').map((s: string) => s.split('=')[1]?.trim()) ??
            [],
        )
        .concat(
          Array.isArray(values.configuration?.feeds)
            ? values.configuration.feeds.map((feed: ISyndicationFeed) => feed.source)
            : [],
        )
        .filter((s, index, all): s is string => !!s && all.indexOf(s) === index),
    [values.source?.code, values.configuration],
  );
  const sourceOptions = React.useMemo(
    () => [
      new OptionItem('All sources for this ingest', ''),
      ...[...ingestSources]
        .sort((a, b) => a.localeCompare(b))
        .map((code) => {
          const source = sources.find((s) => s.code === code);
          return new OptionItem(source ? `${code} - ${source.name}` : code, code);
        }),
    ],
    [ingestSources, sources],
  );

  const fetch = React.useCallback(
    async (filter: IContentReferenceListFilter) => {
      if (!ingestSources.length) return;
      try {
        const start = moment(filter.discoveredOn).startOf('day');
        const query: IIngestContentReferenceFilter = {
          page: filter.pageIndex + 1,
          quantity: filter.pageSize,
          topic: values.topic,
          sources: filter.source ? [filter.source] : ingestSources,
          uid: filter.uid ? filter.uid : undefined,
          status: filter.status ? filter.status : undefined,
          createdStartOn: start.toISOString(),
          createdEndOn: start.clone().add(1, 'day').toISOString(),
          sort: filter.sort.map((s) => `${s.id}${s.desc ? ' desc' : ''}`),
        };
        const result = await findContentReferences(query);
        setPage(
          new Page(
            result.page - 1,
            result.quantity,
            result.items as IIngestContentReference[],
            result.total,
          ),
        );
      } catch {}
    },
    [findContentReferences, ingestSources, values.topic],
  );

  React.useEffect(() => {
    fetch(filter);
  }, [fetch, filter]);

  const handleChangePage = React.useCallback(
    (pi: number, ps?: number) => {
      if (filter.pageIndex !== pi || filter.pageSize !== ps) {
        setFilter({ ...filter, pageIndex: pi, pageSize: ps ?? filter.pageSize });
      }
    },
    [filter],
  );

  const handleChangeSort = React.useCallback(
    (sortBy: Array<SortingRule<IIngestContentReference>>) => {
      const sorts = sortBy.map((sb) => ({ id: sb.id, desc: !!sb.desc }));
      const same =
        sorts.length === filter.sort.length &&
        sorts.every((val, i) => val.id === filter.sort[i]?.id && val.desc === filter.sort[i]?.desc);
      if (!same) {
        setFilter({ ...filter, pageIndex: 0, sort: sorts });
      }
    },
    [filter],
  );

  return (
    <styled.ContentReferenceList>
      <h2>{values.name}</h2>
      <p>
        The content this ingest discovered on the selected day and posted to its Kafka topic{' '}
        <b>{values.topic}</b>, for debugging. Select a headline to open the story in a new tab. A
        reference without imported content shows its UID instead.
      </p>
      <div className="filter">
        <div className="filter-date">
          <SelectDate
            name="discoveredOn"
            label="Discovered On"
            placeholderText="YYYY MM DD"
            selected={filter.discoveredOn}
            width={FieldSize.Stretch}
            popperPlacement="bottom-start"
            onChange={(date) => {
              if (date) setFilter({ ...filter, pageIndex: 0, discoveredOn: date });
            }}
          />
        </div>
        <div className="filter-source">
          <Select
            label="Source"
            name="source"
            width={FieldSize.Stretch}
            options={sourceOptions}
            value={sourceOptions.find((o) => o.value === filter.source) ?? sourceOptions[0]}
            isClearable={false}
            onChange={(newValue: any) => {
              setFilter({ ...filter, pageIndex: 0, source: newValue?.value ?? '' });
            }}
          />
        </div>
        <div className="filter-status">
          <Select
            label="Status"
            name="status"
            width={FieldSize.Stretch}
            options={statusOptions}
            value={statusOptions.find((o) => o.value === filter.status) ?? statusOptions[0]}
            isClearable={false}
            onChange={(newValue: any) => {
              setFilter({ ...filter, pageIndex: 0, status: newValue?.value ?? '' });
            }}
          />
        </div>
        <div className="filter-uid">
          <Text
            key={filter.uid}
            name="uid"
            label="UID"
            width={FieldSize.Stretch}
            defaultValue={filter.uid}
            onBlur={(e) => {
              if (e.target.value !== filter.uid)
                setFilter({ ...filter, pageIndex: 0, uid: e.target.value });
            }}
            onKeyDown={(e) => {
              if (e.key === 'Enter') e.currentTarget.blur();
            }}
          />
        </div>
        <div className="filter-actions">
          <IconButton iconType="search" title="Refresh" onClick={() => fetch(filter)} />
          <IconButton
            iconType="reset"
            title="Reset to today"
            onClick={() =>
              setFilter({ ...defaultContentReferenceFilter, discoveredOn: new Date() })
            }
          />
        </div>
      </div>
      <PagedTable
        columns={contentReferenceColumns}
        page={page}
        isLoading={!(requests.length === 0)}
        sorting={{ sortBy: filter.sort }}
        onChangePage={handleChangePage}
        onChangeSort={handleChangeSort}
      />
    </styled.ContentReferenceList>
  );
};

export default ContentReferenceList;
