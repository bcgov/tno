import React from 'react';
import { FaFolderMinus } from 'react-icons/fa6';

import * as styled from './styled';

export interface IRemoveFromFolderProps {
  onClick: Function;
}
export const RemoveFromFolder: React.FC<IRemoveFromFolderProps> = ({ onClick }) => {
  return (
    <styled.RemoveFromFolder>
      <div className="action" onClick={() => onClick()}>
        <FaFolderMinus /> <span>REMOVE FROM FOLDER</span>
      </div>
    </styled.RemoveFromFolder>
  );
};
