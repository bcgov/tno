import { DateFilter } from 'components/date-filter';
import { ContentListActionBar } from 'components/tool-bar';
import { determineColumns } from 'features/home/constants';
import { filterFormat, getFilterActions } from 'features/search-page/utils';
import { castToSearchResult } from 'features/utils';
import { IContentSearchResult } from 'features/utils/interfaces';
import moment from 'moment';
import React from 'react';
import { useNavigate } from 'react-router-dom';
import { useContent, useLookup } from 'store/hooks';
import {
  ActionName,
  FlexboxTable,
  generateQuery,
  IContentModel,
  ITableInternalRow,
  Row,
} from 'tno-core';

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
  const [{ actions }] = useLookup();

  const [content, setContent] = React.useState<IContentSearchResult[]>([]);
  const [selected, setSelected] = React.useState<IContentModel[]>([]);

  const selectedIds = selected.map((i) => i.id.toString());

  React.useEffect(() => {
    if (!!actions && actions.length > 0) {
      let actionFilters = getFilterActions(actions);
      const commentaryAction = actionFilters[ActionName.Commentary];

      findContentWithElasticsearch(
        generateQuery(
          filterFormat({
            actions: [commentaryAction],
            contentTypes: [],
            startDate: moment(filter.startDate).toISOString(),
            endDate: moment(filter.endDate).toISOString(),
            searchUnpublished: false,
            size: 500,
          }),
        ),
        false,
      )
        .then((res) => {
          setContent(
            res.hits.hits.map((r) => {
              const content = r._source as IContentModel;
              return castToSearchResult(content);
            }),
          );
        })
        .catch();
    }
    // only run this effect when the filter changes
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [actions, filter]);

  /** controls the checking and unchecking of rows in the list view */
  const handleSelectedRowsChanged = (row: ITableInternalRow<IContentSearchResult>) => {
    if (row.isSelected) {
      setSelected(row.table.rows.filter((r) => r.isSelected).map((r) => r.original));
    } else {
      setSelected((selected) => selected.filter((r) => r.id !== row.original.id));
    }
  };

  return (
    <styled.TodaysCommentary>
      <ContentListActionBar
        content={selected}
        onSelectAll={(e) => (e.target.checked ? setSelected(content) : setSelected([]))}
      />
      <DateFilter filter={filter} storeFilter={storeFilter} />
      <Row className="table-container">
        <FlexboxTable
          showHeader={false}
          rowId="id"
          columns={determineColumns('all')}
          isMulti
          onSelectedChanged={handleSelectedRowsChanged}
          selectedRowIds={selectedIds}
          groupBy={(item) => item.original.source?.name ?? ''}
          onRowClick={(e: any) => {
            navigate(`/view/${e.original.id}`);
          }}
          data={content}
          pageButtons={5}
          showPaging={false}
        />
      </Row>
    </styled.TodaysCommentary>
  );
};
