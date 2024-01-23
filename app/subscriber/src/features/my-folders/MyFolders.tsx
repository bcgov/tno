import { SubscriberTableContainer } from 'components/table';
import React from 'react';
import { FaFolderPlus, FaWandMagicSparkles } from 'react-icons/fa6';
import { useNavigate, useParams } from 'react-router-dom';
import { toast } from 'react-toastify';
import { useFolders } from 'store/hooks/subscriber/useFolders';
import { FlexboxTable, IFolderModel, Row, Text } from 'tno-core';

import { columns } from './constants/columns';
import * as styled from './styled';

export interface IMyFoldersProps {
  /** contains a list of the user's folders, allows for edit and viewing */
  myFolders: IFolderModel[];
  /** function to set the user's folders */
  setMyFolders: React.Dispatch<React.SetStateAction<IFolderModel[]>>;
  /** function to set the active folder */
  setActive: React.Dispatch<React.SetStateAction<IFolderModel | undefined>>;
  /** the active folder */
  active?: IFolderModel;
}
/** contains a list of the user's folders, allows for edit and viewing */
export const MyFolders: React.FC<IMyFoldersProps> = ({
  myFolders,
  setMyFolders,
  active,
  setActive,
}) => {
  const [, { findMyFolders, addFolder, updateFolder }] = useFolders();
  const navigate = useNavigate();
  const { id } = useParams();
  const [newFolderName, setNewFolderName] = React.useState<string>('');
  const [editable, setEditable] = React.useState<string>('');

  React.useEffect(() => {
    findMyFolders().then((data) => {
      setMyFolders(data);
    });
    // Only do this on init.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const handleAdd = () => {
    if (newFolderName)
      addFolder({
        name: newFolderName,
        description: '',
        settings: {
          keepAgeLimit: 0,
        },
        isEnabled: true,
        sortOrder: 0,
        id: 0,
        content: [],
        reports: [],
        events: [],
      }).then((data) => {
        toast.success(`${data.name} created successfully`);
        setNewFolderName('');
        setMyFolders([...myFolders, data]);
      });
    else toast.warning(`Folder name is required.`);
  };

  const handleSave = () => {
    if (!!active) {
      updateFolder(active).then((data) => {
        toast.success(`${data.name} updated successfully`);
        setMyFolders([...myFolders.filter((folder) => folder.id !== data.id), data]);
        setEditable('');
      });
    }
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
            columns={columns(editable, handleSave, Number(id), navigate)}
            rowId={'id'}
            onRowClick={(e) => {
              setActive(e.original);
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
