import { ContentStatusName, IContentModel } from 'hooks/api-editor';
import { Column, UseSortByColumnOptions } from 'react-table';
import { Checkbox, Date, Ellipsis } from 'tno-core/dist/components/cell';
import { formatIdirUsername } from 'utils/formatIdir';

export const columns = (minimize: boolean = false) => {
  const columns: (Column<IContentModel> & UseSortByColumnOptions<IContentModel>)[] = [
    {
      id: 'id',
      Header: 'Headline',
      accessor: 'headline',
      width: 5,
      Cell: ({ value }) => <Ellipsis>{value}</Ellipsis>,
    },
    {
      id: 'otherSource',
      Header: 'Source',
      width: 1,
      accessor: 'otherSource',
      Cell: ({ value }) => <Ellipsis>{value}</Ellipsis>,
    },
    {
      id: 'productId',
      Header: 'Type',
      width: 2,
      accessor: (row) => row.product?.name,
      Cell: ({ value }: { value: string }) => <Ellipsis>{value}</Ellipsis>,
    },
    {
      id: 'page',
      Header: 'Section Page',
      width: 1,
      accessor: (row) =>
        row.printContent?.section ? `${row.printContent.section}/${row.page}` : row.page,
      Cell: ({ value }: { value: string }) => <Ellipsis>{value}</Ellipsis>,
    },
    {
      id: 'publishedOn',
      Header: 'Pub Date',
      width: 1,
      accessor: (row) => row.publishedOn ?? row.createdOn,
      Cell: ({ value }: any) => <Date value={value} />,
    },
    {
      id: 'use',
      Header: 'Use',
      disableSortBy: true,
      width: 1,
      accessor: (row) =>
        row.status === ContentStatusName.Publish || row.status === ContentStatusName.Published,
      Cell: ({ value }: { value: boolean }) => {
        return <Checkbox checked={value} />;
      },
    },
  ];

  if (!minimize) {
    columns.splice(4, 0, {
      id: 'ownerId',
      Header: 'Username',
      width: 1,
      accessor: (row) => row.owner?.displayName,
      Cell: ({ value }: { value: string }) => <Ellipsis>{formatIdirUsername(value)}</Ellipsis>,
    });
    columns.splice(5, 0, {
      id: 'status',
      Header: 'Status',
      width: 1,
      accessor: (row) => row.status,
    });
  }

  return columns;
};
