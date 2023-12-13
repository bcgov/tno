import { TooltipMenu } from 'components/tooltip-menu';
import { toFolderContent } from 'components/utils';
import { FolderMenu } from 'features/content/view-content/FolderMenu';
import React from 'react';
import { FaFolderPlus } from 'react-icons/fa6';
import { IContentModel } from 'tno-core';

import * as styled from './styled';

export interface IAddToFolderMenuProps {
  content: IContentModel[];
}

export const AddToFolderMenu: React.FC<IAddToFolderMenuProps> = ({ content }) => {
  return (
    <styled.AddToMenu>
      <div data-tooltip-id="tooltip-add-to-folder" className="action">
        <FaFolderPlus /> <span>ADD TO FOLDER</span>
        <TooltipMenu clickable openOnClick id="tooltip-add-to-folder" place="bottom">
          <FolderMenu content={toFolderContent(content)} />
        </TooltipMenu>
      </div>
    </styled.AddToMenu>
  );
};
