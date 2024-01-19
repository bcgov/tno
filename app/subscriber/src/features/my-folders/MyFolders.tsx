import { SubscriberTableContainer } from 'components/table';
import { TooltipMenu } from 'components/tooltip-menu';
import React from 'react';
import { FaFolderPlus, FaWandMagicSparkles, FaWandSparkles } from 'react-icons/fa6';
import { useNavigate } from 'react-router-dom';
import { toast } from 'react-toastify';
import { useFolders } from 'store/hooks/subscriber/useFolders';
import { Col, FlexboxTable, IFolderModel, Modal, Row, Text, useModal } from 'tno-core';

import { columns } from './constants/columns';
import * as styled from './styled';

/** contains a list of the user's folders, allows for edit and viewing */
export const MyFolders = () => {
  const [, { findMyFolders, addFolder, updateFolder, deleteFolder }] = useFolders();
  const { toggle, isShowing } = useModal();
  const navigate = useNavigate();
  const [myFolders, setMyFolders] = React.useState<IFolderModel[]>([]);
  const [newFolderName, setNewFolderName] = React.useState<string>('');
  const [active, setActive] = React.useState<IFolderModel>();
  const [editable, setEditable] = React.useState<string>('');
  const [actionName, setActionName] = React.useState<'empty' | 'delete'>('delete');

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
            columns={columns(setActive, editable, handleSave, active)}
            rowId={'id'}
            onRowClick={(e) => navigate(`/folders/view/${e.original.id}`)}
            data={myFolders}
            showActive={false}
          />
        </SubscriberTableContainer>
        <TooltipMenu
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
            <div className="option" onClick={() => navigate(`/folders/configure/${active?.id}`)}>
              Configure folder
            </div>
            <div
              className="option"
              onClick={() => {
                setActionName('empty');
                toggle();
              }}
            >
              Empty this folder
            </div>
            <div
              className="option"
              onClick={() => {
                setActionName('delete');
                toggle();
              }}
            >
              Delete this folder
            </div>
          </Col>
        </TooltipMenu>
      </Row>
      <Modal
        headerText="Confirm Removal"
        body={`Are you sure you wish to ${actionName} this folder?`}
        isShowing={isShowing}
        hide={toggle}
        type="delete"
        confirmText="Yes, Remove It"
        onConfirm={() => {
          try {
            if (!!active) {
              if (actionName === 'empty') {
                updateFolder({ ...active, content: [] }).then((data) => {
                  toast.success(`${active.name} updated successfully`);
                  setMyFolders(myFolders.map((item) => (item.id === active.id ? data : item)));
                });
              } else if (actionName === 'delete') {
                deleteFolder(active).then(() => {
                  toast.success(`${active.name} deleted successfully`);
                  setMyFolders(myFolders.filter((folder) => folder.id !== active.id));
                });
              }
            }
          } finally {
            toggle();
          }
        }}
      />
    </styled.MyFolders>
  );
};
