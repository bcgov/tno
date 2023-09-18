import React from 'react';
import { AiOutlineFolderAdd } from 'react-icons/ai';
import { IoAddCircleOutline } from 'react-icons/io5';
import { toast } from 'react-toastify';
import { useFolders } from 'store/hooks/subscriber/useFolders';
import { Col, IContentModel, IFolderModel, Row } from 'tno-core';

import * as styled from './styled';

export interface IFolderMenuProps {
  /** The current content that is being viewed. */
  content?: IContentModel;
}

/** The submenu that appears in the tooltip when clicking on "Add folder" from the content tool bar */
export const FolderMenu: React.FC<IFolderMenuProps> = ({ content }) => {
  const [, { findMyFolders, addFolder, updateFolder }] = useFolders();
  const [myFolders, setMyFolders] = React.useState<IFolderModel[]>([]);
  const [folderName, setFolderName] = React.useState('');

  React.useEffect(() => {
    findMyFolders().then((data) => {
      setMyFolders(data);
    });
    // Only on initialize
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const handleAdd = () => {
    if (!!content) {
      addFolder({
        name: folderName,
        description: '',
        settings: {},
        isEnabled: true,
        sortOrder: 0,
        id: 0,
        content: [{ ...content, sortOrder: 0, contentId: content.id }],
      }).then((data) => {
        toast.success(`${folderName} created and "${content.headline}" added to folder.`);
        setFolderName('');
        setMyFolders([...myFolders, data]);
      });
    }
  };

  const handleUpdate = (folder: IFolderModel) => {
    if (!!content) {
      updateFolder({
        ...folder,
        content: [...folder.content, { ...content, sortOrder: 0, contentId: content.id }],
      }).then((data) => {
        toast.success(`${content.headline} added to folder`);
      });
    }
  };

  return (
    <styled.FolderMenu>
      <p>Add to: </p>
      <Row className="add-row">
        <input
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
