import { FaCog } from 'react-icons/fa';
import { FaFolderClosed, FaFolderOpen } from 'react-icons/fa6';
import { CellEllipsis, IFolderModel, ITableHookColumn, Row } from 'tno-core';

export const columns = (
  setActive: (folder: IFolderModel) => void,
  activeId?: number,
  navigate?: (path: string) => void,
): ITableHookColumn<IFolderModel>[] => [
  {
    label: '',
    accessor: 'name',
    width: 8,
    cell: (cell) => (
      <CellEllipsis>
        <Row
          className={cell.original.id === activeId ? `active-folder-row` : `inactive-folder-row`}
        >
          {activeId === cell.original.id ? <FaFolderOpen /> : <FaFolderClosed />}
          <span className="folder-text">{cell.original.name}</span>
        </Row>
      </CellEllipsis>
    ),
  },
  {
    label: '',
    accessor: 'storyCount',
    width: 0.5,
    cell: (cell) => <CellEllipsis>{cell.original.content.length ?? 0}</CellEllipsis>,
  },
  {
    label: '',
    accessor: 'options',
    width: 0.5,
    cell: (cell) => (
      <FaCog
        onClick={(e) => {
          // stop the row click event from firing
          e.stopPropagation();
          setActive(cell.original);
          navigate && navigate(`/folders/configure/${cell.original.id}`);
        }}
        data-tooltip-id="options"
      />
    ),
  },
];
