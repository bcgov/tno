import { CellCheckbox, CellEllipsis, IFolderModel, ITableHookColumn } from 'tno-core';

export const columns: ITableHookColumn<IFolderModel>[] = [
  {
    label: 'Name',
    accessor: 'name',
    width: 3,
    cell: (cell) => <CellEllipsis>{cell.original.name}</CellEllipsis>,
  },
  {
    label: 'Description',
    accessor: 'description',
    width: 4,
    cell: (cell) => <CellEllipsis>{cell.original.description}</CellEllipsis>,
  },
  {
    label: 'Owner',
    accessor: 'ownerId',
    width: 2,
    cell: (cell) => <CellEllipsis>{cell.original.owner?.username}</CellEllipsis>,
  },
  {
    label: 'Enabled',
    accessor: 'isEnabled',
    width: 1,
    hAlign: 'center',
    cell: (cell) => <CellCheckbox checked={cell.original.isEnabled} />,
  },
];
