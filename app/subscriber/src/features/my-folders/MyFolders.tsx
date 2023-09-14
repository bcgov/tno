import React from 'react';
import { AiOutlineFolderAdd } from 'react-icons/ai';
import { toast } from 'react-toastify';
import { Tooltip } from 'react-tooltip';
import { useFolders } from 'store/hooks/subscriber/useFolders';
import { Col, FlexboxTable, IFolderModel, Modal, Row, Text, useModal } from 'tno-core';

import { columns } from './constants/columns';
import * as styled from './styled';

/** contains a list of the user's folders, allows for edit and viewing */
export const MyFolders = () => {
  const [, { findMyFolders, addFolder, updateFolder, deleteFolder }] = useFolders();
  const [myFolders, setMyFolders] = React.useState<IFolderModel[]>([]);
  const [newFolderName, setNewFolderName] = React.useState<string>('');
  const [active, setActive] = React.useState<IFolderModel>();
  const [editable, setEditable] = React.useState<string>('');

  const { toggle, isShowing } = useModal();

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
        settings: {},
        isEnabled: true,
        sortOrder: 0,
        id: 0,
        content: [],
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
        <div>Create new folder: </div>
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
          onClick={handleAdd}
          disabled={!newFolderName} // Disable button when folder name is empty
        >
          <AiOutlineFolderAdd className="folder-add" />
        </button>
      </Row>
      <Row>
        <FlexboxTable
          pagingEnabled={false}
          columns={columns(setActive, editable, handleSave, active)}
          rowId={'id'}
          data={myFolders}
          showActive={false}
        />
        <Tooltip
          clickable
          openOnClick
          place="right"
          id="options"
          variant="light"
          className="options"
        >
          <Col className="folder-container">
            <div className="option" onClick={() => setEditable(active?.name ?? '')}>
              Edit folder name
            </div>
            <div className="option" onClick={toggle}>
              Empty this folder
            </div>
            <div
              className="option"
              onClick={() => {
                if (!!active) {
                  deleteFolder(active).then(() => {
                    toast.success(`${active.name} deleted successfully`);
                    setMyFolders(myFolders.filter((folder) => folder.id !== active.id));
                  });
                }
              }}
            >
              Delete this folder
            </div>
          </Col>
        </Tooltip>
      </Row>
      <Modal
        headerText="Confirm Removal"
        body="Are you sure you wish to empty this folder?"
        isShowing={isShowing}
        hide={toggle}
        type="delete"
        confirmText="Yes, Remove It"
        onConfirm={() => {
          try {
            if (!!active) {
              updateFolder({ ...active, content: [] }).then(() => {
                toast.success(`${active.name} updated successfully`);
              });
            }
          } finally {
            toggle();
          }
        }}
      />
    </styled.MyFolders>
  );
};
