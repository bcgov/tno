import { TabControl } from 'components/tab-control';
import { FaFeather } from 'react-icons/fa';
import { IContentSearchResult } from 'store/slices';
import {
  CellCheckbox,
  CellDate,
  CellEllipsis,
  ContentStatusName,
  formatIdirUsername,
  ITableHookColumn,
  Row,
  Show,
} from 'tno-core';

import { getStatusText } from '../utils';

export const columns: ITableHookColumn<IContentSearchResult>[] = [
  {
    accessor: 'headline',
    label: (
      <Row gap="0.5rem">
        <TabControl />
        Headline
      </Row>
    ),
    cell: (cell) => (
      <CellEllipsis>
        <Show visible={cell.row.original.hasTranscript}>
          <FaFeather />
        </Show>
        <span>{cell.original.headline}</span>
      </CellEllipsis>
    ),
    width: 5,
  },
  {
    accessor: 'otherSource',
    label: 'Source',
    cell: (cell) => <CellEllipsis>{cell.original.otherSource}</CellEllipsis>,
  },
  {
    accessor: 'product',
    label: 'Product',
    cell: (cell) => <CellEllipsis>{cell.original.product}</CellEllipsis>,
    width: 1,
  },
  {
    accessor: 'section',
    label: 'Page:Section',
    cell: (cell) => {
      const separator = cell.original.page && cell.original.section ? ':' : '';
      const value = `${cell.original.page}${separator}${cell.original.section}`;
      return <CellEllipsis>{value}</CellEllipsis>;
    },
    width: 2,
  },
  {
    accessor: 'owner',
    label: 'User',
    cell: (cell) => <CellEllipsis>{formatIdirUsername(cell.original.owner)}</CellEllipsis>,
    width: 1,
  },
  {
    accessor: 'status',
    label: 'Status',
    hAlign: 'center',
    cell: (cell) => getStatusText(cell.original.status),
  },
  {
    accessor: 'publishedOn',
    label: 'Pub Date',
    cell: (cell) => <CellDate value={cell.original.publishedOn} />,
    width: 3,
    hAlign: 'center',
  },
  {
    accessor: 'status',
    label: 'Use',
    hAlign: 'center',
    cell: (cell) => (
      <div className="center">
        <CellCheckbox
          checked={
            cell.original.status === ContentStatusName.Publish ||
            cell.original.status === ContentStatusName.Published
          }
        />
      </div>
    ),
    width: '55px',
  },
];
