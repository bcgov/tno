import { MsearchMultisearchBody } from '@elastic/elasticsearch/lib/api/types';
import { DateFilter } from 'components/date-filter';
import { ContentListActionBar } from 'components/tool-bar';
import { determineColumns } from 'features/home/constants';
import { IContentSearchResult } from 'features/utils/interfaces';
import React from 'react';
import { useNavigate } from 'react-router-dom';
import { useContent } from 'store/hooks';
import { FlexboxTable, generateQuery, IContentModel, ITableInternalRow, Show } from 'tno-core';

import { PreviousResults } from './PreviousResults';
import * as styled from './styled';

export const FilterMedia: React.FC = () => {
  const navigate = useNavigate();
  const [
    {
      mediaType: { filter },
    },
    { findContentWithElasticsearch, storeMediaTypeFilter: storeFilter },
  ] = useContent();

  const [results, setResults] = React.useState<IContentSearchResult[]>([]);
  const [selected, setSelected] = React.useState<IContentModel[]>([]);

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

  const selectedIds = selected.map((i) => i.id.toString());
  /** controls the checking and unchecking of rows in the list view */
  const handleSelectedRowsChanged = React.useCallback(
    (row: ITableInternalRow<IContentSearchResult>) => {
      if (row.isSelected) {
        setSelected(row.table.rows.filter((r) => r.isSelected).map((r) => r.original));
      } else {
        setSelected((selected) => selected.filter((r) => r.id !== row.original.id));
      }
    },
    [],
  );

  return (
    <styled.FilterMedia>
      <ContentListActionBar
        content={selected}
        onSelectAll={(e) => (e.target.checked ? setSelected(results) : setSelected([]))}
      />
      <DateFilter filter={filter} storeFilter={storeFilter} />
      <FlexboxTable
        isMulti
        rowId="id"
        columns={determineColumns('all')}
        groupBy={(item) => item.original.source?.name ?? ''}
        onRowClick={(e: any) => {
          navigate(`/view/${e.original.id}`);
        }}
        data={results}
        pageButtons={5}
        showPaging={false}
        onSelectedChanged={handleSelectedRowsChanged}
        selectedRowIds={selectedIds}
      />
      <Show visible={!results.length}>
        <PreviousResults results={results} setResults={setResults} />
      </Show>
    </styled.FilterMedia>
  );
};
