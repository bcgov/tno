import { handleEnterPressed } from 'features/utils';
import { FaCog, FaSave } from 'react-icons/fa';
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
            onKeyDown={(e) => handleEnterPressed(e, handleSave)}
            // stop the row click event from firing
            onClick={(e) => e.stopPropagation()}
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
          <FaSave
            onClick={(e) => {
              e.stopPropagation();
              handleSave();
            }}
          />
        ) : (
          <FaCog
            onClick={(e) => {
              // stop the row click event from firing
              e.stopPropagation();
              setActive(cell.original);
            }}
            data-tooltip-id="options"
          />
        )}
      </>
    ),
  },
];
