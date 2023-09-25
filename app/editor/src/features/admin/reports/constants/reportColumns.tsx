import { CellCheckbox, CellEllipsis, IReportModel, ITableHookColumn } from 'tno-core';

export const reportColumns: ITableHookColumn<IReportModel>[] = [
  {
    label: 'Name',
    accessor: 'name',
    width: 2,
    cell: (cell) => <CellEllipsis>{cell.original.name}</CellEllipsis>,
  },
  {
    label: 'Description',
    accessor: 'description',
    width: 5,
    cell: (cell) => <CellEllipsis>{cell.original.description}</CellEllipsis>,
  },
  {
    label: 'Owner',
    accessor: 'ownerId',
    width: 2,
    cell: (cell) => <CellEllipsis>{cell.original.owner?.username}</CellEllipsis>,
  },
  {
    label: 'Public',
    accessor: 'isPublic',
    width: 1,
    hAlign: 'center',
    cell: (cell) => <CellCheckbox checked={cell.original.isPublic} />,
  },
  {
    label: 'Enabled',
    accessor: 'isEnabled',
    width: 1,
    hAlign: 'center',
    cell: (cell) => <CellCheckbox checked={cell.original.isEnabled} />,
  },
];
