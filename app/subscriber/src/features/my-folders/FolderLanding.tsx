import { PageSection } from 'components/section';
import { ManageFolder } from 'features/manage-folder';
import React from 'react';
import { useParams } from 'react-router-dom';
import { Col, IFolderModel, Show } from 'tno-core';

import { ConfigureFolder } from './ConfigureFolder';
import { MyFolders } from './MyFolders';
import * as styled from './styled';

export const FolderLanding: React.FC<{}> = () => {
  const [activeFolder, setActiveFolder] = React.useState<IFolderModel>();
  const [myFolders, setMyFolders] = React.useState<IFolderModel[]>([]);
  // action consits of the param of type /view /configure etc.
  const { action } = useParams();
  return (
    <styled.FolderLanding split={!!action}>
      <Col className="left-side">
        <PageSection header="My Folders" includeHeaderIcon>
          <MyFolders
            myFolders={myFolders}
            setMyFolders={setMyFolders}
            setActive={setActiveFolder}
          />
        </PageSection>
      </Col>
      <Show visible={!!action}>
        <Col className="right-side">
          <Show visible={action === 'view'}>
            <ManageFolder />
          </Show>
          <Show visible={action === 'configure'}>
            <ConfigureFolder
              active={activeFolder}
              myFolders={myFolders}
              setMyFolders={setMyFolders}
            />
          </Show>
        </Col>
      </Show>
    </styled.FolderLanding>
  );
};
