import { PageSection } from 'components/section';
import { ManageFolder } from 'features/manage-folder';
import React from 'react';
import { useParams } from 'react-router-dom';
import { Col, IFolderModel, Show } from 'tno-core';

import { ConfigureFolder } from './ConfigureFolder';
import { FolderProvider } from './FolderContext';
import { MyFolders } from './MyFolders';
import * as styled from './styled';

export const FolderLanding: React.FC<{}> = () => {
  const { action } = useParams();

  return (
    <styled.FolderLanding split={!!action}>
      <FolderProvider>
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
      </FolderProvider>
    </styled.FolderLanding>
  );
};
