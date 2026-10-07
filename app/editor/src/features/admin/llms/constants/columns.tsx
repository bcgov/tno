import { FaCopy, FaTrash } from 'react-icons/fa';
import { CellCheckbox, CellEllipsis, type ILLMModel, type ITableHookColumn } from 'tno-core';

/**
 * LLM grid columns. The action buttons stop their click from bubbling so they do not also open the
 * LLM (row click).
 * @param onDuplicate Copy the LLM.
 * @param onDelete Remove the LLM.
 * @param disabled Whether the action buttons are disabled.
 * @returns Columns
 */
export const getColumns = (
  onDuplicate: (llm: ILLMModel) => void,
  onDelete: (llm: ILLMModel) => void,
  disabled: boolean = false,
): Array<ITableHookColumn<ILLMModel>> => [
  {
    label: 'Name',
    accessor: 'name',
    width: 2,
    cell: (cell) => <CellEllipsis>{cell.original.name}</CellEllipsis>,
  },
  {
    label: 'Deployment Name',
    accessor: 'deploymentName',
    width: 3,
    cell: (cell) => <CellEllipsis>{cell.original.deploymentName}</CellEllipsis>,
  },
  {
    label: 'Agent',
    accessor: 'agentName',
    width: 2,
    cell: (cell) => <CellEllipsis>{cell.original.agentName}</CellEllipsis>,
  },
  {
    label: 'Public',
    accessor: 'isPublic',
    width: 1,
    hAlign: 'center',
    cell: (cell) => <CellCheckbox checked={!!cell.original.isPublic} />,
  },
  {
    label: 'Enabled',
    accessor: 'isEnabled',
    width: 1,
    hAlign: 'center',
    cell: (cell) => <CellCheckbox checked={cell.original.isEnabled} />,
  },
  {
    label: '',
    accessor: 'id',
    width: 0.6,
    hAlign: 'center',
    showSort: false,
    cell: (cell) => (
      <div className="actions">
        <button
          type="button"
          className="action-button"
          aria-label={`Duplicate ${cell.original.name}`}
          title="Duplicate"
          disabled={disabled}
          onClick={(event) => {
            event.stopPropagation();
            onDuplicate(cell.original);
          }}
        >
          <FaCopy />
        </button>
        <button
          type="button"
          className="action-button delete"
          aria-label={`Delete ${cell.original.name}`}
          title="Delete"
          disabled={disabled}
          onClick={(event) => {
            event.stopPropagation();
            onDelete(cell.original);
          }}
        >
          <FaTrash />
        </button>
      </div>
    ),
  },
];
