import { ITopicModel } from 'hooks/api-editor';
import { Column, UseFiltersColumnOptions, UseSortByColumnOptions } from 'react-table';
import { CellEllipsis } from 'tno-core';

export const columns: (Column<ITopicModel> &
  UseSortByColumnOptions<ITopicModel> &
  UseFiltersColumnOptions<ITopicModel>)[] = [
  {
    id: 'id',
    Header: 'Name',
    accessor: 'name',
    width: 2,
    Cell: ({ value }) => <CellEllipsis>{value}</CellEllipsis>,
  },
  {
    Header: 'Description',
    accessor: 'description',
    width: 3,
    Cell: ({ value }) => <CellEllipsis>{value}</CellEllipsis>,
  },
  {
    Header: 'Type',
    accessor: 'topicType',
    width: 1,
  },
];
