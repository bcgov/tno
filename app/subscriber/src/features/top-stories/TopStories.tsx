import { DateFilter } from 'components/date-filter';
import { FolderSubMenu } from 'components/folder-sub-menu';
import { determineColumns } from 'features/home/constants';
import moment from 'moment';
import React from 'react';
import { useNavigate } from 'react-router-dom';
import { useContent } from 'store/hooks';
import { ActionName, FlexboxTable, IContentModel, ITableInternalRow, Row } from 'tno-core';

import * as styled from './styled';

/** Component that displays top stories defaulting to today's date and adjustable via a date filter. */
export const TopStories: React.FC = () => {
  const [{ homeFilter }, { findContent }] = useContent();
  const navigate = useNavigate();
  const [topStories, setTopStories] = React.useState<IContentModel[]>([]);
  const [selected, setSelected] = React.useState<IContentModel[]>([]);

  React.useEffect(() => {
    findContent({
      actions: [ActionName.TopStory],
      contentTypes: [],
      publishedStartOn: moment(homeFilter.publishedStartOn).toISOString(),
      publishedEndOn: moment(homeFilter.publishedEndOn).toISOString(),
      quantity: 100,
      sort: ['source.sortOrder'],
    })
      .then((data) => setTopStories(data.items))
      .catch(() => {});
  }, [findContent, homeFilter]);

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
      <FolderSubMenu selectedContent={selected} />
      <DateFilter />
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
