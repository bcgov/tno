import { DateFilter } from 'components/date-filter';
import { ContentListActionBar } from 'components/tool-bar';
import { determineColumns } from 'features/home/constants';
import { filterFormat } from 'features/search-page/utils';
import { castToSearchResult } from 'features/utils';
import React from 'react';
import { useNavigate } from 'react-router-dom';
import { useContent, useLookup } from 'store/hooks';
import { Col, FlexboxTable, generateQuery, IContentModel, ITableInternalRow, Row } from 'tno-core';

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
  const [{ actions }] = useLookup();

  const [content, setContent] = React.useState<IContentModel[]>([]);
  const [selected, setSelected] = React.useState<IContentModel[]>([]);

  const selectedIds = selected.map((i) => i.id.toString());

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
      setContent(
        res.hits.hits.map((r) => {
          const content = r._source as IContentModel;
          return castToSearchResult(content);
        }),
      );
    });
    // only run this effect when the filter changes
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [filter, actions]);

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
      <ContentListActionBar
        content={selected}
        onSelectAll={(e) => (e.target.checked ? setSelected(content) : setSelected([]))}
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
          data={content}
          pageButtons={5}
          onSelectedChanged={handleSelectedRowsChanged}
          selectedRowIds={selectedIds}
          showPaging={false}
        />
      </Row>
    </styled.TopStories>
  );
};
