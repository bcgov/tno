import _ from 'lodash';
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
  /** Callback to clear the selected content. */
  onClear?: () => void;
}

/** The submenu that appears in the tooltip when clicking on "Add folder" from the content tool bar */
export const FolderMenu: React.FC<IFolderMenuProps> = ({ content, onClear }) => {
  const [{ myFolders }, { findMyFolders, addFolder, updateFolder }] = useFolders();
  const [folderName, setFolderName] = React.useState('');
  const [{ myReports }, { storeReportContent, storeMyReports }] = useProfileStore();

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
          onClear?.();
        })
        .catch(() => {});
    }
  }, [addFolder, content, folderName, onClear]);

  const handleUpdate = React.useCallback(
    async (folder: IFolderModel) => {
      if (!content?.length) toast.error('No content selected');
      if (!!content?.length) {
        await updateFolder(
          {
            ...folder,
            content: getDistinct([...folder.content, ...content], (item) => item.contentId).map(
              (c, index) => ({ ...c, sortOrder: index }),
            ),
          },
          true,
        )
          .then((folder) => {
            // Update reports that use this folder.
            const results = myReports.map((report) => {
              // If a report is linked to this folder than remove the instances so that the next time the user views it, it will load from the API.
              let clearInstances = false;

              const sections = report.sections.map((section) => {
                if (section.folderId === folder.id) {
                  storeReportContent((reports) => {
                    let result = { ...reports };
                    result[report.id] = _.uniq([
                      ...result[report.id],
                      ...folder.content.map((content) => content.contentId),
                    ]);
                    return result;
                  });
                  section.folder = folder;
                  clearInstances = true;
                }
                return section;
              });
              return {
                ...report,
                sections: sections,
                instances: clearInstances ? [] : report.instances,
              };
            });
            storeMyReports(results);

            let navUrl = `folders/${folder.id}`;
            toast.success(() => (
              <div>
                {content.length} storie(s) added to folder:
                <Link to={navUrl}>{folder.name}</Link>
              </div>
            ));
            onClear?.();
          })
          .catch(() => {});
      }
    },
    [content, myReports, storeMyReports, storeReportContent, updateFolder, onClear],
  );

  return (
    <styled.FolderMenu>
      <Row className="title-row">
        <FaPen /> Create new folder:
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
          Add to folder:
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
