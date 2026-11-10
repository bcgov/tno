import React from 'react';
import { FaCog } from 'react-icons/fa';
import { FaFolderClosed, FaFolderOpen } from 'react-icons/fa6';
import { useNavigate, useParams } from 'react-router-dom';
import { IFolderModel, Row } from 'tno-core';

import * as styled from './styled';

export interface IFolderListProps {
  folders: IFolderModel[];
}
export const FolderList: React.FC<IFolderListProps> = ({ folders }) => {
  const navigate = useNavigate();
  const { id } = useParams();
  return (
    <styled.FolderList>
      {Array.from(folders)
        .sort((a, b) => a.id - b.id)
        .map((f) => (
          <Row
            key={f.id}
            className={`folder-row ${Number(id) === f.id && 'active'}`}
            onClick={() => navigate(`/folders/view/${f.id}`)}
          >
            <Row>
              {Number(id) === f.id ? <FaFolderOpen /> : <FaFolderClosed />}
              <div className="folder-name">{f.name}</div>
            </Row>
            <div className="story-count">{f.content.length}</div>
            <FaCog
              className="settings"
              data-tooltip-id="main-tooltip"
              data-tooltip-content="Configure folder"
              onClick={(e) => {
                // stop the row click event from firing
                e.stopPropagation();
                navigate && navigate(`/folders/configure/${f.id}`);
              }}
            />
          </Row>
        ))}
    </styled.FolderList>
  );
};
