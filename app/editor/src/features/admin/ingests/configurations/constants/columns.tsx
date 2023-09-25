import { FaTrash } from 'react-icons/fa';
import { Button, ButtonVariant, FormikText, ITableHookColumn } from 'tno-core';

export const columns = (
  onClick: (event: any) => {},
  onChange: (event: any, cell: any, isSource?: boolean) => void,
): ITableHookColumn<any>[] => {
  return [
    {
      label: 'Name',
      accessor: 'name',
      width: 5,
      cell: (cell) => {
        return (
          <FormikText
            type="text"
            name="name"
            title="name"
            value={cell.original.name}
            onChange={(e: any) => onChange(e, cell, false)}
            style={{ width: '280px' }}
          />
        );
      },
    },
    {
      label: 'Source',
      accessor: 'source',
      width: 3,
      cell: (cell) => {
        return (
          <FormikText
            type="text"
            name="source"
            title="source"
            value={cell.original.source}
            onChange={(e: any) => onChange(e, cell)}
            style={{ width: '180px' }}
          />
        );
      },
    },
    {
      label: 'Remove',
      accessor: 'Remove',
      width: '1',
      cell: (cell) => (
        <>
          <Button variant={ButtonVariant.danger} onClick={() => onClick(cell.original.id)}>
            <FaTrash className="indicator" />
          </Button>
        </>
      ),
    },
  ];
};
