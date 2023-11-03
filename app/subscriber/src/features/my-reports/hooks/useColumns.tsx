import { FaEdit, FaFileAlt, FaTrash } from 'react-icons/fa';
import { FaChartPie, FaGear } from 'react-icons/fa6';
import { Link } from 'react-router-dom';
import { useNavigate } from 'react-router-dom';
import { CellEllipsis, Checkbox, IReportModel, ITableHookColumn, Row, Show } from 'tno-core';

import { isAutoSend, isAutoSendDisabled, setAutoSend } from '../utils';

export const useColumns = (
  handleUpdate: (report: IReportModel) => void,
  handleDelete: (report: IReportModel) => void,
): ITableHookColumn<IReportModel>[] => {
  const navigate = useNavigate();

  return [
    {
      label: 'Auto Send',
      accessor: 'auto-send',
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
      accessor: 'name',
      width: 2,
      cell: (cell) => (
        <Row gap="0.5rem" alignItems="center">
          <Show visible={cell.original.sections.some((section) => section.settings.showCharts)}>
            <FaChartPie className="primary-light-color" />
          </Show>
          <Show visible={!cell.original.sections.some((section) => section.settings.showCharts)}>
            <FaFileAlt className="primary-light-color" />
          </Show>
          <CellEllipsis>
            <Link to={`/reports/${cell.original.id}/edit`} title="Edit">
              {cell.original.name}
            </Link>
          </CellEllipsis>
        </Row>
      ),
    },
    {
      label: '',
      accessor: 'options',
      width: '100px',
      cell: (cell) => (
        <Row gap="0.5rem">
          <FaGear
            className="btn-link"
            onClick={(e) => {
              e.stopPropagation();
              navigate(`/reports/${cell.original.id}`);
            }}
            title="Configure"
          />
          <FaEdit
            className="btn-link"
            onClick={(e) => {
              e.stopPropagation();
              navigate(`/reports/${cell.original.id}/edit`);
            }}
            title="Edit"
          />
          <FaTrash
            className="btn-link error"
            onClick={(e) => {
              e.stopPropagation();
              handleDelete(cell.original);
            }}
            title="Delete"
          />
        </Row>
      ),
    },
  ];
};
