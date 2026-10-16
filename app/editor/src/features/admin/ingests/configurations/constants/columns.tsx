import { ComboBox, type IComboOption } from 'features/admin/automation/designer';
import { FaTrash } from 'react-icons/fa';
import { Button, ButtonVariant, FieldSize, FormikText, type ITableHookColumn } from 'tno-core';

export interface ISourceMappingItem {
  id: number;
  name: string;
  source: string;
}

/**
 * Columns for the syndication source mapping.
 * @param onClick Removes the row with the specified id.
 * @param onChange Updates the name or source of a row.
 * @param sources Source options, the value is the source code.
 * @returns Table columns.
 */
export const columns = (
  onClick: (id: number) => void,
  onChange: (id: number, field: 'name' | 'source', value: string) => void,
  sources: IComboOption[],
): Array<ITableHookColumn<ISourceMappingItem>> => {
  return [
    {
      label: 'Name',
      accessor: 'name',
      width: 1,
      cell: (cell) => {
        return (
          <FormikText
            type="text"
            name="name"
            title="name"
            value={cell.original.name}
            width={FieldSize.Stretch}
            onChange={(e: any) => {
              const pointer = e.target.selectionStart;
              window.requestAnimationFrame(() => {
                e.target.selectionEnd = pointer;
              });
              onChange(cell.original.id, 'name', e.target.value);
            }}
          />
        );
      },
    },
    {
      label: 'Source',
      accessor: 'source',
      width: 1,
      cell: (cell) => {
        return (
          <ComboBox
            name="source"
            aria-label="Source"
            placeholder="Source code"
            value={cell.original.source}
            suggestions={sources}
            isClearable
            onChange={(value) => onChange(cell.original.id, 'source', value)}
          />
        );
      },
    },
    {
      label: 'Remove',
      accessor: 'Remove',
      width: '70px',
      hAlign: 'center',
      cell: (cell) => (
        <Button variant={ButtonVariant.danger} onClick={() => onClick(cell.original.id)}>
          <FaTrash className="indicator" />
        </Button>
      ),
    },
  ];
};
