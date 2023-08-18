import { FaTrash } from 'react-icons/fa';
import { Button, ButtonVariant, ITableHookColumn } from 'tno-core';

export const columns = (
  onClick: (event: any) => {},
  onChange: (event: any, cell: any, isSource?: boolean) => void,
): ITableHookColumn<any>[] => {
  return [
    {
      label: 'Name',
      name: 'name',
      width: 5,
      cell: (cell) => {
        return (
          <input
            type="text"
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
      name: 'source',
      width: 3,
      cell: (cell) => {
        return (
          <input
            type="text"
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
      name: 'Remove',
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
