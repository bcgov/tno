import { FaExternalLinkAlt } from 'react-icons/fa';
import {
  CellEllipsis,
  Checkbox,
  ContentStatusName,
  IContentModel,
  ITableHookColumn,
} from 'tno-core';

import { getStatusText } from '../../list-view/utils';

const published = [ContentStatusName.Publish, ContentStatusName.Published];

const changeStatus = (status: ContentStatusName) => {
  if (published.includes(status)) return ContentStatusName.Unpublish;
  return ContentStatusName.Publish;
};

export const getColumns = (
  openTab: boolean,
  onClickOpen: (contentId: number) => void,
  onClickUse: (content: IContentModel) => void,
): ITableHookColumn<IContentModel>[] => [
  {
    name: 'headline',
    label: 'Headline',
    cell: (cell) => (
      <CellEllipsis data-tooltip-id="main-tooltip" data-tooltip-content={cell.original.headline}>
        {cell.original.headline}
      </CellEllipsis>
    ),
    width: 6,
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
    label: 'Page:Section',
    cell: (cell) => {
      const value = `${cell.original.page ? `${cell.original.page}:` : ''}${cell.original.section}`;
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
    name: 'status',
    label: 'Use',
    cell: (cell) => (
      <div className="center">
        <Checkbox
          name="publish"
          id={`publish-${cell.original.id}`}
          value={true}
          checked={published.includes(cell.original.status)}
          onChange={() =>
            onClickUse?.({ ...cell.original, status: changeStatus(cell.original.status) })
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
