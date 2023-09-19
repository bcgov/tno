import { FolderMenu } from 'features/content/view-content/FolderMenu';
import { FaFolderPlus } from 'react-icons/fa';
import { Tooltip } from 'react-tooltip';
import { IContentModel, IFolderContentModel, Row } from 'tno-core';

import * as styled from './styled';

export interface IFolderSubMenuProps {
  /**
   * The content that has been selected to add to a folder
   */
  selectedContent: IContentModel[];
}

/** Component that will take selected content, convert it to folder content and render the folder menu. */
export const FolderSubMenu: React.FC<IFolderSubMenuProps> = ({ selectedContent }) => {
  /** transform the content to folder content before sending it to the API */
  const toFolderContent = (content: IContentModel[]) => {
    return content.map((item) => {
      return {
        ...item,
        sortOrder: 0,
        contentId: item.id,
      } as IFolderContentModel;
    });
  };

  return (
    <styled.FolderSubMenu>
      <Row justifyContent="end">
        <FaFolderPlus className="add-folder" data-tooltip-id="folder" />
      </Row>
      <Tooltip
        clickable
        variant="light"
        className="folder-menu"
        place="bottom"
        openOnClick
        style={{ opacity: '1', boxShadow: '0 0 8px #464545', zIndex: '999' }}
        id="folder"
      >
        <FolderMenu content={toFolderContent(selectedContent)} />
      </Tooltip>
    </styled.FolderSubMenu>
  );
};
