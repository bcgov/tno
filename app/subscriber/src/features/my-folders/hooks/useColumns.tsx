import React from 'react';
import { FaCog } from 'react-icons/fa';
import { FaFolderClosed, FaFolderOpen } from 'react-icons/fa6';
import { useNavigate, useParams } from 'react-router-dom';
import { CellEllipsis, IFolderModel, ITableHookColumn, Row } from 'tno-core';

export const useColumns = (): ITableHookColumn<IFolderModel>[] => {
  const navigate = useNavigate();
  const { id } = useParams();

  const folderId = id ? Number(id) : undefined;

  return React.useMemo(
    () => [
      {
        label: '',
        accessor: 'name',
        width: 8,
        cell: (cell) => (
          <CellEllipsis>
            <Row
              className={
                cell.original.id === folderId ? `active-folder-row` : `inactive-folder-row`
              }
            >
              {folderId === cell.original.id ? <FaFolderOpen /> : <FaFolderClosed />}
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
              navigate && navigate(`/folders/configure/${cell.original.id}`);
            }}
            data-tooltip-id="options"
          />
        ),
      },
    ],
    [folderId, navigate],
  );
};
