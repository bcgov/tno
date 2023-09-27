import { FiMoreHorizontal, FiSave } from 'react-icons/fi';
import { CellEllipsis, IFolderModel, ITableHookColumn, Text } from 'tno-core';

export const columns = (
  setActive: (folder: IFolderModel) => void,
  editable: string,
  handleSave: () => void,
  active?: IFolderModel,
): ITableHookColumn<IFolderModel>[] => [
  {
    label: 'My Folders',
    accessor: 'name',
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
    label: 'Story Count',
    accessor: 'storyCount',
    width: 5,
    cell: (cell) => <CellEllipsis>{cell.original.content.length ?? 0}</CellEllipsis>,
  },
  {
    label: '',
    accessor: 'options',
    width: 1,
    cell: (cell) => (
      <>
        {editable === cell.original.name ? (
          <FiSave onClick={() => handleSave()} className="elips" />
        ) : (
          <FiMoreHorizontal
            onClick={(e) => {
              // stop the row click event from firing
              e.stopPropagation();
              setActive(cell.original);
            }}
            data-tooltip-id="options"
            className="elips"
          />
        )}
      </>
    ),
  },
];
