import { Checkbox } from 'components';
import { ContentStatus, IContentModel } from 'hooks/api-editor';
import moment from 'moment';
import { Column } from 'react-table';

const checkboxColumn = ({ value }: { value: boolean }) => (
  <Checkbox defaultChecked={value} value={value ? 'true' : 'false'} />
);

const dateColumn = ({ value }: { value: Date }) => <>{moment(value).format('MM/DD/YYYY')}</>;

export const columns: Column<IContentModel>[] = [
  {
    Header: 'Headline',
    accessor: 'headline',
  },
  {
    Header: 'Page',
    accessor: 'page',
  },
  {
    Header: 'Username',
    accessor: (row) => row.owner?.displayName,
  },
  {
    Header: 'Status',
    accessor: (row) => ContentStatus[row.status],
  },
  {
    Header: 'Source',
    accessor: 'source',
  },
  {
    Header: 'Section',
    accessor: 'section',
  },
  {
    Header: 'Type',
    accessor: (row) => row.mediaType.name,
  },
  {
    Header: 'Date',
    accessor: 'date',
    Cell: dateColumn,
  },
  {
    Header: 'Use',
    accessor: 'use',
    Cell: checkboxColumn,
  },
];
