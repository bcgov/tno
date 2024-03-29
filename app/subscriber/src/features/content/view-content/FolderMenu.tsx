import React from 'react';
import { FaFolderPlus, FaPen } from 'react-icons/fa6';
import { toast } from 'react-toastify';
import { useFolders } from 'store/hooks/subscriber/useFolders';
import { useProfileStore } from 'store/slices';
import {
  Button,
  Col,
  getDistinct,
  IFolderContentModel,
  IFolderModel,
  Link,
  Row,
  Text,
} from 'tno-core';

import * as styled from './styled';

export interface IFolderMenuProps {
  /** The current content that is being viewed. */
  content?: IFolderContentModel[];
}

/** The submenu that appears in the tooltip when clicking on "Add folder" from the content tool bar */
export const FolderMenu: React.FC<IFolderMenuProps> = ({ content }) => {
  const [{ myFolders }, { findMyFolders, addFolder, updateFolder }] = useFolders();
  const [folderName, setFolderName] = React.useState('');
  const [{ myReports }, { storeReportContent }] = useProfileStore();

  React.useEffect(() => {
    if (!myFolders.length) findMyFolders().catch(() => {});
    // Only on initialize
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const handleAdd = React.useCallback(async () => {
    if (!!content) {
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
      })
        .then((folder) => {
          let navUrl = `folders/${folder.id}`;
          toast.success(() => (
            <div>
              Folder
              <Link to={navUrl}>{folder.name}</Link> created and {content.length} storie(s) added to
              folder.
            </div>
          ));
          setFolderName('');
        })
        .catch(() => {});
    }
  }, [addFolder, content, folderName]);

  const handleUpdate = React.useCallback(
    async (folder: IFolderModel) => {
      if (!content?.length) toast.error('No content selected');
      if (!!content?.length) {
        await updateFolder({
          ...folder,
          content: getDistinct([...folder.content, ...content], (item) => item.contentId).map(
            (c, index) => ({ ...c, sortOrder: index }),
          ),
        })
          .then(() => {
            // Update reports that use this folder.
            myReports.forEach((report) => {
              report.sections.forEach((section) => {
                if (section.folderId === folder.id) {
                  storeReportContent((reports) => {
                    let result = { ...reports };
                    result[report.id] = folder.content.map((content) => content.contentId);
                    return result;
                  });
                }
              });
            });
            let navUrl = `folders/${folder.id}`;
            toast.success(() => (
              <div>
                {content.length} storie(s) added to folder:
                <Link to={navUrl}>{folder.name}</Link>
              </div>
            ));
          })
          .catch(() => {});
      }
    },
    [content, myReports, storeReportContent, updateFolder],
  );

  return (
    <styled.FolderMenu>
      <Row className="title-row">
        <FaPen /> Create new folder:{' '}
      </Row>
      <Row className="add-row">
        <Text
          placeholder="Enter a folder name..."
          className="folder-name"
          name="folder"
          onChange={(e) => setFolderName(e.target.value)}
        />
        <Button
          disabled={folderName.length === 0}
          className="add-folder"
          onClick={() => handleAdd()}
        >
          Create
        </Button>
      </Row>
      <Col>
        <Row className="add-title">
          <FaFolderPlus />
          Add to folder:{' '}
        </Row>
        {myFolders.map((folder) => {
          return (
            <Row className="folder-row" key={folder.id} onClick={() => handleUpdate(folder)}>
              <div className="row-item">{folder.name}</div>
            </Row>
          );
        })}
      </Col>
    </styled.FolderMenu>
  );
};
