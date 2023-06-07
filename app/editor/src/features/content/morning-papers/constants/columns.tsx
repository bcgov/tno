import { FaExternalLinkAlt } from 'react-icons/fa';
import {
  CellCheckbox,
  CellDate,
  CellEllipsis,
  ContentStatusName,
  IContentModel,
  ITableHookColumn,
} from 'tno-core';

import { getStatusText } from '../../list-view/utils';

export const getColumns = (
  openTab: boolean,
  onClickOpen: (contentId: number) => void,
): ITableHookColumn<IContentModel>[] => [
  {
    name: 'headline',
    label: 'Headline',
    cell: (cell) => (
      <CellEllipsis data-tooltip-id="main-tooltip" data-tooltip-content={cell.original.headline}>
        {cell.original.headline}
      </CellEllipsis>
    ),
    width: 5,
  },
  {
    name: 'otherSource',
    label: 'Source',
    cell: (cell) => <CellEllipsis>{cell.original.otherSource}</CellEllipsis>,
  },
  {
    name: 'seriesId',
    label: 'Product',
    cell: (cell) => (
      <CellEllipsis
        data-tooltip-id="main-tooltip"
        data-tooltip-content={cell.original.product?.name}
      >
        {cell.original.product?.name}
      </CellEllipsis>
    ),
    width: 2,
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
    hAlign: 'center',
  },
  {
    name: 'newTab',
    label: '',
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
