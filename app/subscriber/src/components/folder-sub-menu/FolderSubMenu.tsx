import { FolderMenu } from 'features/content/view-content/FolderMenu';
import { Tooltip } from 'react-tooltip';
import { IContentModel, IFolderContentModel } from 'tno-core';

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
  );
};
