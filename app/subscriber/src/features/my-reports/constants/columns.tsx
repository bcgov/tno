import { FiMoreHorizontal, FiSave } from 'react-icons/fi';
import { CellEllipsis, Checkbox, IReportModel, ITableHookColumn, Text } from 'tno-core';

export const columns = (
  setActive: (folder: IReportModel) => void,
  editable: string,
  handleSave: () => void,
  active?: IReportModel,
): ITableHookColumn<IReportModel>[] => [
  {
    label: 'Auto Send',
    name: 'auto-send',
    width: 1,
    cell: (cell) => <Checkbox />,
  },
  {
    label: 'Name',
    name: 'name',
    width: 2,
    cell: (cell) => (
      <CellEllipsis>
        {active && editable === cell.original.name ? (
          <Text
            className="re-name"
            name="name"
            value={active.name}
            onChange={(e) => setActive({ ...active, name: e.target.value })}
            key={active.id}
          />
        ) : (
          cell.original.name
        )}
      </CellEllipsis>
    ),
  },
  {
    label: '',
    name: 'options',
    width: 1,
    cell: (cell) => (
      <>
        {editable === cell.original.name ? (
          <FiSave onClick={() => handleSave()} className="elips" />
        ) : (
          <FiMoreHorizontal
            onClick={() => setActive(cell.original)}
            data-tooltip-id="options"
            className="elips"
          />
        )}
      </>
    ),
  },
];
