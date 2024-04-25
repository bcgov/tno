import { PageSection } from 'components/section';
import { ManageFolder } from 'features/manage-folder';
import React from 'react';
import { useParams } from 'react-router-dom';
import { Col, Show } from 'tno-core';

import { ConfigureFolder } from './ConfigureFolder';
import { MyFolderContextProvider } from './MyFolderContext';
import { MyFolders } from './MyFolders';
import * as styled from './styled';

export const FolderLanding: React.FC<{}> = () => {
  const { action } = useParams();

  return (
    <MyFolderContextProvider>
      <styled.FolderLanding split={!!action}>
        <Col className="left-side">
          <PageSection header="My Folders" includeHeaderIcon>
            <MyFolders />
          </PageSection>
        </Col>
        <Show visible={!!action}>
          <Col className="right-side">
            <Show visible={action === 'view'}>
              <ManageFolder />
            </Show>
            <Show visible={action === 'configure'}>
              <ConfigureFolder />
            </Show>
          </Col>
        </Show>
      </styled.FolderLanding>
    </MyFolderContextProvider>
  );
};
