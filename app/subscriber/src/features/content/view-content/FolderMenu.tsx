import React from 'react';
import { AiOutlineFolderAdd } from 'react-icons/ai';
import { IoAddCircleOutline } from 'react-icons/io5';
import { toast } from 'react-toastify';
import { useFolders } from 'store/hooks/subscriber/useFolders';
import { Col, getDistinct, IFolderContentModel, IFolderModel, Row, Text } from 'tno-core';

import * as styled from './styled';

export interface IFolderMenuProps {
  /** The current content that is being viewed. */
  content?: IFolderContentModel[];
}

/** The submenu that appears in the tooltip when clicking on "Add folder" from the content tool bar */
export const FolderMenu: React.FC<IFolderMenuProps> = ({ content }) => {
  const [{ myFolders }, { findMyFolders, addFolder, updateFolder }] = useFolders();
  const [folderName, setFolderName] = React.useState('');

  React.useEffect(() => {
    if (!myFolders.length) findMyFolders().catch(() => {});
    // Only on initialize
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const handleAdd = React.useCallback(async () => {
    if (!!content) {
      try {
        await addFolder({
          name: folderName,
          description: '',
          settings: {
            keepAgeLimit: 0,
          },
          isEnabled: true,
          sortOrder: 0,
          id: 0,
          content: content,
          reports: [],
          events: [],
        });

        toast.success(`${folderName} created and ${content.length} stories added to folder.`);
        setFolderName('');
      } catch {}
    }
  }, [addFolder, content, folderName]);

  const handleUpdate = React.useCallback(
    async (folder: IFolderModel) => {
      if (!content?.length) toast.error('No content selected');
      if (!!content?.length) {
        try {
          await updateFolder({
            ...folder,
            content: getDistinct([...folder.content, ...content], (item) => item.contentId).map(
              (c, index) => ({ ...c, sortOrder: index }),
            ),
          });

          toast.success(`${content.length} stories added to folder`);
        } catch {}
      }
    },
    [content, updateFolder],
  );

  return (
    <styled.FolderMenu>
      <p>Add to: </p>
      <Row className="add-row">
        <Text
          placeholder="Create new folder..."
          className="folder-name"
          name="folder"
          onChange={(e) => setFolderName(e.target.value)}
        />
        <AiOutlineFolderAdd onClick={() => handleAdd()} className="popout-icon" />
      </Row>
      <Col>
        {myFolders.map((folder) => {
          return (
            <Row className="folder-row" key={folder.id}>
              <div className="row-item">{folder.name}</div>
              <IoAddCircleOutline onClick={() => handleUpdate(folder)} className="popout-icon" />
            </Row>
          );
        })}
      </Col>
    </styled.FolderMenu>
  );
};
