import { FaExternalLinkAlt } from 'react-icons/fa';
import {
  type Column,
  type UseFiltersColumnOptions,
  type UseSortByColumnOptions,
} from 'react-table';
import { CellDate } from 'tno-core';

import { type IIngestContentReference } from '../interfaces';

export const contentReferenceColumns: Array<
  Column<IIngestContentReference> &
    UseSortByColumnOptions<IIngestContentReference> &
    UseFiltersColumnOptions<IIngestContentReference>
> = [
  {
    Header: 'Discovered On',
    accessor: 'createdOn',
    Cell: (cell) => <CellDate value={cell.value} />,
    width: 2,
  },
  {
    Header: 'Source',
    accessor: 'source',
    width: 1,
  },
  {
    Header: 'Headline',
    id: 'headline',
    accessor: 'headline',
    disableSortBy: true,
    width: 5,
    // Opens the imported content, or shows the uid when the reference has no content.
    Cell: (cell) =>
      cell.row.original.contentId ? (
        <a
          href={`/contents/${cell.row.original.contentId}`}
          target="_blank"
          rel="noreferrer"
          title="Open the content in a new tab"
        >
          {cell.value || cell.row.original.uid} <FaExternalLinkAlt />
        </a>
      ) : (
        <span className="not-imported" title="No content was imported for this reference">
          {cell.row.original.uid}
        </span>
      ),
  },
  {
    Header: 'Published On',
    accessor: 'publishedOn',
    Cell: (cell) => <CellDate value={cell.value} />,
    width: 2,
  },
  {
    Header: 'Status',
    accessor: 'status',
    width: 1,
  },
];
