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

/** Component that renders the button that gives users access to a sub menu that will allow them to add selected content to
 * an existing folder. Or create a new one.
 */
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
    <styled.FolderSubMenu className="folder-sub-menu">
      <Row justifyContent="end">
        <FaFolderPlus className="add-folder" data-tooltip-id="folder" />
      </Row>
      <Tooltip
        clickable
        variant="light"
        className="folder-menu"
        place="bottom"
        openOnClick
        opacity={1}
        style={{ boxShadow: '0 0 8px #464545', zIndex: '999' }}
        id="folder"
      >
        <FolderMenu content={toFolderContent(selectedContent)} />
      </Tooltip>
    </styled.FolderSubMenu>
  );
};
