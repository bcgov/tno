import { PageSection } from 'components/section';
import { ManageFolder } from 'features/manage-folder';
import React from 'react';
import { useParams } from 'react-router-dom';
import { Col, Show } from 'tno-core';

import { ConfigureFolder } from './ConfigureFolder';
import { FolderProvider } from './FolderContext';
import { MyFolders } from './MyFolders';
import * as styled from './styled';

export const FolderLanding: React.FC<{}> = () => {
  const { action, id } = useParams();
  const [folderAction, setFolderAction] = React.useState('');

  React.useEffect(() => {
    if (!!action && !!id) {
      setFolderAction(action);
    }
    if (action === undefined && id) {
      setFolderAction('view');
    }
  }, [action, id]);

  return (
    <styled.FolderLanding split={!!folderAction}>
      <FolderProvider>
        <Col className="left-side">
          <PageSection header="My Folders" includeHeaderIcon>
            <MyFolders />
          </PageSection>
        </Col>
        <Show visible={!!folderAction}>
          <Col className="right-side">
            <Show visible={folderAction === 'view'}>
              <ManageFolder />
            </Show>
            <Show visible={folderAction === 'configure'}>
              <ConfigureFolder />
            </Show>
          </Col>
        </Show>
      </FolderProvider>
    </styled.FolderLanding>
  );
};
