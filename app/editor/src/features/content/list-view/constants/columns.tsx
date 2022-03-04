import { Checkbox } from 'components/form';
import { ContentStatus, IContentModel } from 'hooks/api-editor';
import moment from 'moment';
import { Column, UseSortByColumnOptions } from 'react-table';

const checkboxColumn = ({ value }: { value: boolean }) => (
  <Checkbox defaultChecked={value} value={value ? 'true' : 'false'} />
);

const dateColumn = ({ value }: { value: IContentModel }) => {
  const created = moment(value.createdOn);
  const text = created.isValid() ? created.format('MM/DD/YYYY') : '';
  return <>{text}</>;
};

export const columns: (Column<IContentModel> & UseSortByColumnOptions<IContentModel>)[] = [
  {
    id: 'headline',
    Header: 'Headline',
    accessor: 'headline',
  },
  {
    id: 'source',
    Header: 'Source',
    accessor: 'source',
  },
  {
    id: 'mediaType',
    Header: 'Type',
    accessor: (row) => row.mediaType?.name,
  },
  {
    id: 'section',
    Header: 'Section/Page',
    accessor: (row) => {
      if (row.printContent?.section) return `${row.printContent.section}/${row.page}`;
      return row.page;
    },
  },
  {
    id: 'ownerId',
    Header: 'Username',
    accessor: (row) => row.owner?.displayName,
  },
  {
    id: 'status',
    Header: 'Status',
    accessor: (row) => row.status,
  },
  {
    id: 'createdOn',
    Header: 'Date',
    accessor: (row) => row,
    Cell: dateColumn,
  },
  {
    id: 'use',
    Header: 'Use',
    disableSortBy: true,
    accessor: (row) =>
      row.status === ContentStatus.Publish || row.status === ContentStatus.Published,
    Cell: checkboxColumn,
  },
];
