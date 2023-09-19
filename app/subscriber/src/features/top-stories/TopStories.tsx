import { DateFilter } from 'components/date-filter';
import { FolderSubMenu } from 'components/folder-sub-menu';
import { determineColumns } from 'features/home/constants';
import moment from 'moment';
import React from 'react';
import { FaFolderPlus } from 'react-icons/fa';
import { useNavigate } from 'react-router-dom';
import { useContent } from 'store/hooks';
import { ActionName, FlexboxTable, IContentModel, ITableInternalRow, Row } from 'tno-core';

import * as styled from './styled';

/** Component that displays top stories defaulting to today's date and adjustable via a date filter. */
export const TopStories: React.FC = () => {
  const [{ filterAdvanced }, { findContent }] = useContent();
  const navigate = useNavigate();
  const [topStories, setTopStories] = React.useState<IContentModel[]>([]);
  const [selected, setSelected] = React.useState<IContentModel[]>([]);

  React.useEffect(() => {
    findContent({
      actions: [ActionName.TopStory],
      contentTypes: [],
      publishedStartOn: moment(filterAdvanced.startDate).toISOString(),
      publishedEndOn: moment(filterAdvanced.endDate).toISOString(),
      quantity: 100,
    }).then((data) => setTopStories(data.items));
  }, [findContent, filterAdvanced]);

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
      <Row justifyContent="end">
        <FaFolderPlus className="add-folder" data-tooltip-id="folder" />
      </Row>
      <DateFilter />
      <FolderSubMenu selectedContent={selected} />
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
