import { CellCheckbox, CellEllipsis, ISettingModel, ITableHookColumn } from 'tno-core';

export const columns: ITableHookColumn<ISettingModel>[] = [
  {
    label: 'Name',
    accessor: 'name',
    width: 2,
    cell: (cell) => <CellEllipsis>{cell.original.name}</CellEllipsis>,
  },
  {
    label: 'Description',
    accessor: 'description',
    width: 2,
    cell: (cell) => <CellEllipsis>{cell.original.description}</CellEllipsis>,
  },
  {
    label: 'Value',
    accessor: 'value',
    width: 2,
    hAlign: 'center',
    cell: (cell) => cell.original.value,
  },
  {
    label: 'Enabled',
    accessor: 'isEnabled',
    width: 1,
    hAlign: 'center',
    cell: (cell) => <CellCheckbox checked={cell.original.isEnabled} />,
  },
];
