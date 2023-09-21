import { FaEdit, FaTrash } from 'react-icons/fa';
import { FaGear } from 'react-icons/fa6';
import { Link } from 'react-router-dom';
import { useNavigate } from 'react-router-dom';
import { CellEllipsis, Checkbox, IReportModel, ITableHookColumn, Row } from 'tno-core';

import { isAutoSend, isAutoSendDisabled, setAutoSend } from '../utils';

export const useColumns = (
  handleUpdate: (report: IReportModel) => void,
  handleDelete: (report: IReportModel) => void,
): ITableHookColumn<IReportModel>[] => {
  const navigate = useNavigate();

  return [
    {
      label: 'Auto Send',
      name: 'auto-send',
      width: '40px',
      cell: (cell) => (
        <Checkbox
          checked={isAutoSend(cell.original)}
          disabled={isAutoSendDisabled(cell.original)}
          onChange={(e) => handleUpdate(setAutoSend(cell.original, e.target.checked))}
        />
      ),
    },
    {
      label: 'Name',
      name: 'name',
      width: 2,
      cell: (cell) => (
        <CellEllipsis>
          <Link to={`/reports/${cell.original.id}/edit`} title="Edit">
            {cell.original.name}
          </Link>
        </CellEllipsis>
      ),
    },
    {
      label: '',
      name: 'options',
      width: '80px',
      cell: (cell) => (
        <Row gap="0.5rem">
          <FaGear
            className="btn-link"
            onClick={() => navigate(`/reports/${cell.original.id}`)}
            title="Configure"
          />
          <FaEdit
            className="btn-link"
            onClick={() => navigate(`/reports/${cell.original.id}/edit`)}
            title="Edit"
          />
          <FaTrash
            className="btn-link error"
            onClick={() => handleDelete(cell.original)}
            title="Delete"
          />
        </Row>
      ),
    },
  ];
};
