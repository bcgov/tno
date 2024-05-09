import { FaRegClipboard } from 'react-icons/fa';
import { CellCheckbox, CellEllipsis, IFilterModel, ITableHookColumn } from 'tno-core';
const handleCopyKeyWords = (event: any, cell: any) => {
  navigator.clipboard.writeText(cell.original.settings!.search);
  // animate the clipboar icon to show something happened
  event.target.classList.toggle('animate');
  setTimeout(() => {
    event.target.classList.toggle('animate');
  }, 200);
};

export const filterColumns: ITableHookColumn<IFilterModel>[] = [
  {
    label: 'Name',
    accessor: 'name',
    width: 3,
    cell: (cell) => <CellEllipsis>{cell.original.name}</CellEllipsis>,
  },
  {
    label: 'Description',
    accessor: 'description',
    width: 4,
    cell: (cell) => <CellEllipsis>{cell.original.description}</CellEllipsis>,
  },
  {
    label: 'Owner',
    accessor: 'ownerId',
    width: 2,
    cell: (cell) => <CellEllipsis>{cell.original.owner?.username}</CellEllipsis>,
  },
  {
    label: 'Keywords',
    accessor: 'keywords',
    width: 2,
    cell: (cell) => {
      return (
        <div className="keyword-cell">
          <CellEllipsis>{cell.original.settings?.search}</CellEllipsis>
          {cell.original.settings?.search ? (
            <FaRegClipboard
              className="clipboard-icon"
              title="Copy keywords to clipboard"
              onClick={(e) => {
                e.stopPropagation();
                handleCopyKeyWords(e, cell);
              }}
            />
          ) : null}
        </div>
      );
    },
  },
  {
    label: 'Enabled',
    accessor: 'isEnabled',
    width: 1,
    hAlign: 'center',
    cell: (cell) => <CellCheckbox checked={cell.original.isEnabled} />,
  },
];
