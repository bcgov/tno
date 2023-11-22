import { DateFilter } from 'components/date-filter';
import { FolderSubMenu } from 'components/folder-sub-menu';
import { determineColumns } from 'features/home/constants';
import moment from 'moment';
import React from 'react';
import { useNavigate } from 'react-router-dom';
import { useContent } from 'store/hooks';
import { ActionName, FlexboxTable, IContentModel, ITableInternalRow, Row } from 'tno-core';

import * as styled from './styled';

/** Component that displays commentary defaulting to today's date and adjustable via a date filter. */
export const TodaysCommentary: React.FC = () => {
  const [{ homeFilter: filter }, { findContent }] = useContent();
  const navigate = useNavigate();
  const [commentary, setCommentary] = React.useState<IContentModel[]>([]);
  const [selected, setSelected] = React.useState<IContentModel[]>([]);

  React.useEffect(() => {
    findContent({
      actions: [ActionName.Commentary],
      contentTypes: [],
      publishedStartOn: moment(filter.publishedStartOn).toISOString(),
      publishedEndOn: moment(filter.publishedEndOn).toISOString(),
      quantity: 100,
    }).then((data) => setCommentary(data.items));
  }, [findContent, filter]);

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
      <FolderSubMenu selectedContent={selected} />
      <DateFilter />
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
