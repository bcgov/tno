import { FaCog } from 'react-icons/fa';
import { CellEllipsis, Checkbox, IProductSubscriberModel, ITableHookColumn, Row } from 'tno-core';

export const useColumns = (
  handleToggleSubscription: (report: IProductSubscriberModel) => void,
): ITableHookColumn<IProductSubscriberModel>[] => {
  return [
    {
      label: 'Subscribed',
      accessor: 'isSubscribed',
      width: 1,
      cell: (cell) => <Checkbox defaultChecked={cell.original.isSubscribed} disabled />,
    },
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
      label: '',
      accessor: 'options',
      width: '100px',
      cell: (cell) => (
        <Row gap="0.5rem">
          <FaCog
            className="button-link"
            onClick={() => handleToggleSubscription(cell.original)}
            title={cell.original.isSubscribed ? 'Unsubscribe' : 'Subscribe'}
          />
        </Row>
      ),
    },
  ];
};
