import { DateFilter } from 'components/date-filter';
import { ContentActionBar } from 'components/tool-bar';
import { determineColumns } from 'features/home/constants';
import { filterFormat } from 'features/search-page/utils';
import { castToSearchResult } from 'features/utils';
import moment from 'moment';
import React from 'react';
import { useNavigate } from 'react-router-dom';
import { useContent, useLookup } from 'store/hooks';
import { FlexboxTable, generateQuery, IContentModel, ITableInternalRow, Row } from 'tno-core';

import * as styled from './styled';

/** Component that displays commentary defaulting to today's date and adjustable via a date filter. */
export const TodaysCommentary: React.FC = () => {
  const [
    {
      todaysCommentary: { filter },
    },
    { findContentWithElasticsearch, storeTodayCommentaryFilter: storeFilter },
  ] = useContent();
  const navigate = useNavigate();
  const [commentary, setCommentary] = React.useState<IContentModel[]>([]);
  const selectAllZone = document.querySelector('.content');
  const [selected, setSelected] = React.useState<IContentModel[]>([]);
  const [{ actions }] = useLookup();
  const [selectAll, setSelectAll] = React.useState(false);
  React.useEffect(() => {
    findContentWithElasticsearch(
      generateQuery(
        filterFormat(
          {
            commentary: true,
            contentTypes: [],
            startDate: moment(filter.startDate).toISOString(),
            endDate: moment(filter.endDate).toISOString(),
            searchUnpublished: false,
            size: 500,
          },
          actions,
        ),
      ),
      false,
    )
      .then((res) => {
        setCommentary(
          res.hits.hits.map((r) => {
            const content = r._source as IContentModel;
            return castToSearchResult(content);
          }),
        );
      })
      .catch();
    // only run this effect when the filter changes
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [filter]);

  /** when select all is toggled all items are selected */
  React.useEffect(() => {
    if (selectAll) setSelected(commentary);
    if (!selectAll) setSelected([]);
  }, [commentary, selectAll]);

  /** controls the checking and unchecking of rows in the list view */
  const handleSelectedRowsChanged = (row: ITableInternalRow<IContentModel>) => {
    if (row.isSelected) {
      setSelected(row.table.rows.filter((r) => r.isSelected).map((r) => r.original));
    } else {
      setSelected((selected) => selected.filter((r) => r.id !== row.original.id));
    }
  };

  return (
    <styled.TodaysCommentary>
      <ContentActionBar
        content={selected}
        onList
        setSelectAll={setSelectAll}
        selectAllZone={selectAllZone ?? undefined}
      />
      <DateFilter filter={filter} storeFilter={storeFilter} />
      <Row className="table-container">
        <FlexboxTable
          rowId="id"
          columns={determineColumns('all')}
          isMulti
          onSelectedChanged={handleSelectedRowsChanged}
          groupBy={(item) => item.original.source?.name ?? ''}
          onRowClick={(e: any) => {
            navigate(`/view/${e.original.id}`);
          }}
          data={commentary || []}
          pageButtons={5}
          showPaging={false}
        />
      </Row>
    </styled.TodaysCommentary>
  );
};
