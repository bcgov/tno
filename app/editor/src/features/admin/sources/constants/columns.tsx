import { CellCheckbox, CellEllipsis, ISourceModel, ITableHookColumn, Link } from 'tno-core';

export const columns: ITableHookColumn<ISourceModel>[] = [
  {
    label: 'Name',
    accessor: 'name',
    width: 3,
    cell: (cell) => (
      <Link to={`${cell.original.id}`}>
        <CellEllipsis>{cell.original.name}</CellEllipsis>
      </Link>
    ),
  },
  {
    label: 'Code',
    accessor: 'code',
    width: 2,
    cell: (cell) => <CellEllipsis>{cell.original.code}</CellEllipsis>,
  },
  {
    label: 'Order',
    accessor: 'sortOrder',
    width: 1,
    hAlign: 'center',
    cell: (cell) => cell.original.sortOrder,
  },
  {
    label: 'Topics',
    accessor: 'useInTopics',
    width: 1,
    hAlign: 'center',
    cell: (cell) => <CellCheckbox checked={cell.original.useInTopics} />,
  },
  {
    label: 'Paper',
    accessor: 'configuration.isDailyPaper',
    width: 1,
    hAlign: 'center',
    cell: (cell) => <CellCheckbox checked={cell.original.configuration.isDailyPaper} />,
  },
  {
    label: 'Enabled',
    accessor: 'isEnabled',
    width: 1,
    hAlign: 'center',
    cell: (cell) => <CellCheckbox checked={cell.original.isEnabled} />,
  },
];
