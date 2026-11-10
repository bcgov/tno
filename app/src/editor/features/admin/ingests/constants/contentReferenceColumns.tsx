import { FaExternalLinkAlt } from 'react-icons/fa';

import { CellDate, type ITableHookColumn } from '@/shared';

import { type IIngestContentReference } from '../interfaces';

export const contentReferenceColumns: Array<ITableHookColumn<IIngestContentReference>> = [
  {
    label: 'Discovered On',
    accessor: 'createdOn',
    sort: 'createdOn',
    width: 2,
    cell: (cell) => <CellDate value={cell.original.createdOn} />,
  },
  {
    label: 'Source',
    accessor: 'source',
    sort: 'source',
    width: 1,
    cell: (cell) => <>{cell.original.source}</>,
  },
  {
    label: 'Headline',
    accessor: 'headline',
    showSort: false,
    width: 5,
    // Opens the imported content, or shows the uid when the reference has no content.
    cell: (cell) =>
      cell.original.contentId ? (
        <a
          href={`/contents/${cell.original.contentId}`}
          target="_blank"
          rel="noreferrer"
          title="Open the content in a new tab"
        >
          {cell.original.headline || cell.original.uid} <FaExternalLinkAlt />
        </a>
      ) : (
        <span className="not-imported" title="No content was imported for this reference">
          {cell.original.uid}
        </span>
      ),
  },
  {
    label: 'Published On',
    accessor: 'publishedOn',
    sort: 'publishedOn',
    width: 2,
    cell: (cell) => <CellDate value={cell.original.publishedOn} />,
  },
  {
    label: 'Status',
    accessor: 'status',
    sort: 'status',
    width: 1,
    cell: (cell) => <>{cell.original.status}</>,
  },
];
