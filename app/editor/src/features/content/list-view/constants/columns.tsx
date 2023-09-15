import { FaExternalLinkAlt, FaFeather } from 'react-icons/fa';
import { IContentSearchResult } from 'store/slices';
import {
  CellCheckbox,
  CellDate,
  CellEllipsis,
  ContentStatusName,
  formatIdirUsername,
  ITableHookColumn,
  Show,
} from 'tno-core';

import { getStatusText } from '../utils';

export const getColumns = (
  openTab: boolean,
  onClickOpen: (contentId: number) => void,
): ITableHookColumn<IContentSearchResult>[] => [
  {
    name: 'headline',
    label: 'Headline',
    cell: (cell) => (
      <CellEllipsis data-tooltip-id="main-tooltip" data-tooltip-content={cell.original.headline}>
        <Show visible={cell.row.original.hasTranscript}>
          <FaFeather />
        </Show>
        <span>{cell.original.headline}</span>
      </CellEllipsis>
    ),
    width: 5,
  },
  {
    name: 'otherSource',
    label: 'Source',
    cell: (cell) => (
      <CellEllipsis data-tooltip-id="main-tooltip" data-tooltip-content={cell.original.otherSource}>
        {cell.original.otherSource}
      </CellEllipsis>
    ),
  },
  {
    name: 'product',
    label: 'Product',
    cell: (cell) => (
      <CellEllipsis data-tooltip-id="main-tooltip" data-tooltip-content={cell.original.product}>
        {cell.original.product}
      </CellEllipsis>
    ),
    width: 1,
  },
  {
    name: 'section',
    label: 'Section:Page',
    cell: (cell) => {
      const value = `${cell.original.section ? `${cell.original.section}:` : ''}${
        cell.original.page
      }`;
      return (
        <CellEllipsis data-tooltip-id="main-tooltip" data-tooltip-content={value}>
          {value}
        </CellEllipsis>
      );
    },
    width: 2,
  },
  {
    name: 'owner',
    label: 'User',
    cell: (cell) => (
      <CellEllipsis data-tooltip-id="main-tooltip" data-tooltip-content={cell.original.owner}>
        {formatIdirUsername(cell.original.owner)}
      </CellEllipsis>
    ),
    width: 1,
  },
  {
    name: 'status',
    label: 'Status',
    hAlign: 'center',
    cell: (cell) => getStatusText(cell.original.status),
  },
  {
    name: 'publishedOn',
    label: 'Pub Date',
    cell: (cell) => <CellDate value={cell.original.publishedOn} />,
    width: 3,
    hAlign: 'center',
  },
  {
    name: 'status',
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
  {
    name: 'newTab',
    label: '',
    // Header: () => (
    //   <FaInfoCircle data-tooltip-id="main-tooltip" data-tooltip-content="Open snippet in new tab" />
    // ),
    showSort: false,
    hAlign: 'center',
    isVisible: !openTab,
    cell: (cell) => {
      return (
        <FaExternalLinkAlt
          onClick={(e) => {
            e.preventDefault();
            e.stopPropagation();
            onClickOpen(cell.original.id);
          }}
        />
      );
    },
    width: '50px',
  },
];
