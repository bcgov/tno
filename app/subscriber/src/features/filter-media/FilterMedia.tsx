import { MsearchMultisearchBody } from '@elastic/elasticsearch/lib/api/types';
import { DateFilter } from 'components/date-filter';
import { determineColumns } from 'features/home/constants';
import moment from 'moment';
import React, { useMemo } from 'react';
import { FiRefreshCcw } from 'react-icons/fi';
import { useNavigate } from 'react-router-dom';
import { useContent, useLookup } from 'store/hooks';
import { FieldSize, FlexboxTable, generateQuery, IContentModel, Row, Select } from 'tno-core';

import * as styled from './styled';

export const FilterMedia: React.FC = () => {
  const navigate = useNavigate();
  const [{ mediaTypes, sources }] = useLookup();
  const [
    {
      mediaType: { filter },
    },
    { findContentWithElasticsearch, storeMediaTypeFilter: storeFilter },
  ] = useContent();

  const mediaTypeOptions = useMemo(
    () =>
      mediaTypes.map((t) => {
        return { value: t.id, label: t.name };
      }),
    [mediaTypes],
  );

  const sourceOptions = useMemo(() => {
    return sources.map((s) => {
      return { value: s.id, label: s.name };
    });
  }, [sources]);

  const [results, setResults] = React.useState<IContentModel[]>([]);

  const fetchResults = React.useCallback(
    async (filter: MsearchMultisearchBody) => {
      try {
        const res: any = await findContentWithElasticsearch(filter, false);
        setResults(res.hits.hits.map((h: { _source: IContentModel }) => h._source));
      } catch {}
    },
    // only run on filter change
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [filter],
  );

  React.useEffect(() => {
    // stops invalid requests before filter is synced with date
    if (!filter.startDate) return;
    fetchResults(
      generateQuery({
        ...filter,
        mediaTypeIds: filter.mediaTypeIds ?? [],
        sourceIds: filter.sourceIds ?? [],
      }),
    );
    // only run on filter change
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [filter]);

  return (
    <styled.FilterMedia>
      <Row className="tool-bar">
        <Select
          width={FieldSize.Medium}
          key={`${filter.mediaTypeIds?.length}-media`}
          name="select-media-type"
          placeholder={'Select a media type'}
          defaultValue={mediaTypeOptions.filter((o) => {
            return filter.mediaTypeIds?.includes(Number(o.value));
          })}
          isClearable={false}
          onChange={(e: any) => {
            if (!!e.value) {
              storeFilter({ ...filter, mediaTypeIds: [e.value] });
            }
          }}
          options={mediaTypeOptions}
        />
        <Select
          isClearable={false}
          options={sourceOptions}
          placeholder={'Select a source'}
          key={`${filter.sourceIds?.length}-source`}
          defaultValue={sourceOptions.filter((o) => {
            return filter.sourceIds?.includes(Number(o.value));
          })}
          onChange={(e: any) => {
            if (!!e.value) {
              storeFilter({ ...filter, sourceIds: [e.value] });
            }
          }}
          name="source-select"
          width={FieldSize.Medium}
        />
        <DateFilter filter={filter} storeFilter={storeFilter} />
        <FiRefreshCcw
          className="reset"
          onClick={() => {
            storeFilter({
              ...filter,
              mediaTypeIds: [],
              sourceIds: [],
              startDate: moment().startOf('day').toISOString(),
              endDate: moment().endOf('day').toISOString(),
            });
          }}
        />
      </Row>
      <Row className="table-container">
        <FlexboxTable
          rowId="id"
          columns={determineColumns('all')}
          groupBy={(item) => item.original.source?.name ?? ''}
          onRowClick={(e: any) => {
            navigate(`/view/${e.original.id}`);
          }}
          data={results}
          pageButtons={5}
          showPaging={false}
        />
      </Row>
    </styled.FilterMedia>
  );
};
