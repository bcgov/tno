import { Checkbox, IProductModel, ITableHookColumn, Link } from 'tno-core';

export const reportProductColumns: ITableHookColumn<IProductModel>[] = [
  {
    label: 'Subscribed',
    accessor: 'subscribed',
    width: 1,
    cell: (cell) => <Checkbox />,
  },
  {
    label: 'Name',
    accessor: 'name',
    width: 2,
    cell: (cell) => (
      <Link to={`/reports/${cell.original.id}/view`} title="Edit">
        {cell.original.name}
      </Link>
    ),
  },
  // {
  //   label: 'Start time',
  //   accessor: 'start-time',
  //   width: 1,
  //   cell: () => '--',
  // },
];
