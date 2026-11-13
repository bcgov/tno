import React from 'react';
import { FaFolderPlus } from 'react-icons/fa6';

import { IContentModel } from '@/shared';
import { TooltipMenu } from '@/subscriber/components/tooltip-menu';
import { toFolderContent } from '@/subscriber/components/utils';
import { FolderMenu } from '@/subscriber/features/content/view-content/FolderMenu';

import * as styled from './styled';

export interface IAddToFolderMenuProps {
  content: IContentModel[];
  /** Callback to clear the selected content. */
  onClear?: () => void;
}

export const AddToFolderMenu: React.FC<IAddToFolderMenuProps> = ({ content, onClear }) => {
  return (
    <styled.AddToMenu>
      <div data-tooltip-id="tooltip-add-to-folder" className="action">
        <FaFolderPlus /> <span>ADD TO FOLDER</span>
        <TooltipMenu clickable openOnClick id="tooltip-add-to-folder" place="bottom">
          <FolderMenu content={toFolderContent(content)} onClear={onClear} />
        </TooltipMenu>
      </div>
    </styled.AddToMenu>
  );
};
