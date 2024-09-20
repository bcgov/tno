import React from 'react';
import { FaFolderPlus, FaWandMagicSparkles } from 'react-icons/fa6';
import { toast } from 'react-toastify';
import { useApp } from 'store/hooks';
import { useFolders } from 'store/hooks/subscriber/useFolders';
import { Col, Loader, Row, Text } from 'tno-core';

import { FolderList } from './FolderList';
import * as styled from './styled';

export interface IMyFoldersProps {}

/** contains a list of the user's folders, allows for edit and viewing */
export const MyFolders: React.FC<IMyFoldersProps> = () => {
  const [{ userInfo }] = useApp();
  const [{ myFolders }, { findMyFolders, addFolder }] = useFolders();
  const [loading, setLoading] = React.useState(false);
  const [newFolderName, setNewFolderName] = React.useState<string>('');

  React.useEffect(() => {
    if (userInfo && !myFolders.length) {
      setLoading(true);
      findMyFolders()
        .then(() => {})
        .catch(() => {})
        .finally(() => {
          setLoading(false);
        });
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [userInfo]);

  const handleAdd = () => {
    if (newFolderName)
      addFolder({
        name: newFolderName,
        description: '',
        settings: {
          keepAgeLimit: 0,
          autoPopulate: false,
        },
        isEnabled: true,
        sortOrder: 0,
        id: 0,
        content: [],
        reports: [],
        events: [],
      })
        .then((data) => {
          setNewFolderName('');
          toast.success(`${data.name} created successfully`);
        })
        .catch(() => {});
    else toast.warning(`Folder name is required.`);
  };

  return (
    <styled.MyFolders>
      <Col className="create-new">
        <Row className="create-label">
          <FaWandMagicSparkles className="wand" />
          <div className="create-text">CREATE NEW FOLDER: </div>
        </Row>
        <Row>
          <Text
            name="folderName"
            className="folder-name"
            placeholder="Enter name..."
            value={newFolderName}
            onChange={(e) => setNewFolderName(e.target.value)}
          />
          <button
            type="button"
            title="Create new folder"
            className="create-button"
            onClick={handleAdd}
            disabled={!newFolderName} // Disable button when folder name is empty
          >
            <FaFolderPlus className="folder-add" />
          </button>
        </Row>
      </Col>
      <Loader visible={loading} />
      <Row>
        <FolderList folders={myFolders} />
      </Row>
    </styled.MyFolders>
  );
};
