import { SubscriberTableContainer } from 'components/table';
import React from 'react';
import { FaFolderPlus, FaWandMagicSparkles } from 'react-icons/fa6';
import { useNavigate } from 'react-router-dom';
import { toast } from 'react-toastify';
import { useApp } from 'store/hooks';
import { useFolders } from 'store/hooks/subscriber/useFolders';
import { FlexboxTable, Row, Text } from 'tno-core';

import { useColumns } from './hooks';
import * as styled from './styled';

export interface IMyFoldersProps {}

/** contains a list of the user's folders, allows for edit and viewing */
export const MyFolders: React.FC<IMyFoldersProps> = () => {
  const navigate = useNavigate();
  const [{ userInfo }] = useApp();
  const [{ myFolders }, { findMyFolders, addFolder }] = useFolders();
  const columns = useColumns();

  const [newFolderName, setNewFolderName] = React.useState<string>('');

  React.useEffect(() => {
    if (userInfo && !myFolders.length) findMyFolders().catch(() => {});
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
      }).then((data) => {
        setNewFolderName('');
        toast.success(`${data.name} created successfully`);
      });
    else toast.warning(`Folder name is required.`);
  };

  return (
    <styled.MyFolders>
      <Row className="create-new">
        <FaWandMagicSparkles className="wand" />
        <div className="create-text">CREATE NEW FOLDER: </div>
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
      <Row>
        <SubscriberTableContainer>
          <FlexboxTable
            pagingEnabled={false}
            columns={columns}
            rowId={'id'}
            disableZebraStriping
            onRowClick={(e) => {
              navigate(`/folders/view/${e.original.id}`);
            }}
            data={myFolders}
            showActive={false}
          />
        </SubscriberTableContainer>
      </Row>
    </styled.MyFolders>
  );
};
