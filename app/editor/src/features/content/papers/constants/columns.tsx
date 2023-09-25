import { TabControl } from 'components/tab-control';
import { IContentSearchResult } from 'store/slices';
import { CellEllipsis, Checkbox, ContentStatusName, ITableHookColumn, Row } from 'tno-core';

import { getStatusText } from '../../list-view/utils';

const published = [ContentStatusName.Publish, ContentStatusName.Published];

const changeStatus = (status: ContentStatusName) => {
  if (published.includes(status)) return ContentStatusName.Unpublish;
  return ContentStatusName.Publish;
};

export const getColumns = (
  onClickUse: (content: IContentSearchResult) => void,
): ITableHookColumn<IContentSearchResult>[] => [
  {
    accessor: 'headline',
    label: (
      <Row gap="0.5rem">
        <TabControl />
        Headline
      </Row>
    ),
    cell: (cell) => <CellEllipsis>{cell.original.headline}</CellEllipsis>,
    width: 6,
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
    width: 2,
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
    accessor: 'status',
    label: 'Status',
    hAlign: 'center',
    cell: (cell) => getStatusText(cell.original.status),
  },
  {
    accessor: 'status',
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
];
