import { DateFilter } from 'components/date-filter';
import { ContentActionBar } from 'components/tool-bar';
import { determineColumns } from 'features/home/constants';
import { filterFormat } from 'features/search-page/utils';
import { castToSearchResult } from 'features/utils';
import React from 'react';
import { useNavigate } from 'react-router-dom';
import { useContent, useLookup } from 'store/hooks';
import { FlexboxTable, generateQuery, IContentModel, ITableInternalRow, Row } from 'tno-core';

import * as styled from './styled';

/** Component that displays top stories defaulting to today's date and adjustable via a date filter. */
export const TopStories: React.FC = () => {
  const [
    {
      topStories: { filter },
    },
    { findContentWithElasticsearch, storeTopStoriesFilter: storeFilter },
  ] = useContent();
  const navigate = useNavigate();
  const [topStories, setTopStories] = React.useState<IContentModel[]>([]);
  const [selectAll, setSelectAll] = React.useState(false);
  const [selected, setSelected] = React.useState<IContentModel[]>([]);
  const [{ actions }] = useLookup();
  const selectAllZone = document.querySelector('.content');
  React.useEffect(() => {
    // stops invalid requests before filter is synced with date
    if (!actions.length || !filter.startDate) return;
    findContentWithElasticsearch(
      generateQuery(
        filterFormat(
          {
            ...filter,
            topStory: true,
          },
          actions,
        ),
      ),
      false,
    ).then((res) => {
      setTopStories(
        res.hits.hits.map((r) => {
          const content = r._source as IContentModel;
          return castToSearchResult(content);
        }),
      );
    });
    // only run this effect when the filter changes
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [filter, actions]);

  /** when select all is toggled all items are selected */
  React.useEffect(() => {
    if (selectAll) setSelected(topStories);
    if (!selectAll) setSelected([]);
  }, [topStories, selectAll]);

  /** controls the checking and unchecking of rows in the list view */
  const handleSelectedRowsChanged = (row: ITableInternalRow<IContentModel>) => {
    if (row.isSelected) {
      setSelected(row.table.rows.filter((r) => r.isSelected).map((r) => r.original));
    } else {
      setSelected((selected) => selected.filter((r) => r.id !== row.original.id));
    }
  };

  return (
    <styled.TopStories>
      <ContentActionBar
        content={selected}
        setSelectAll={setSelectAll}
        onList
        selectAllZone={selectAllZone ?? undefined}
      />
      <DateFilter filter={filter} storeFilter={storeFilter} />
      <Row className="table-container">
        <FlexboxTable
          rowId="id"
          columns={determineColumns('all')}
          isMulti
          groupBy={(item) => item.original.source?.name ?? ''}
          onRowClick={(e: any) => {
            navigate(`/view/${e.original.id}`);
          }}
          data={topStories}
          pageButtons={5}
          onSelectedChanged={handleSelectedRowsChanged}
          showPaging={false}
        />
      </Row>
    </styled.TopStories>
  );
};
