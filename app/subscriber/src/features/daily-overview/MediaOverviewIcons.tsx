import { PageSection } from 'components/section';
import React from 'react';
import { FaFeather, FaVideo } from 'react-icons/fa6';

import * as styled from './styled';

export const MediaOverviewIcons: React.FC = () => {
  return (
    <styled.MediaOverviewIcons>
      <PageSection header="Media Overview Icons">
        <div className="content">
          <div>
            <FaFeather className="transcript-icon" />
            <span>Headlines with this icon feature a transcript. Click icon to view.</span>
          </div>
          <div>
            <FaVideo className="video-icon" />
            <span>Story includes a video. Click icon to view.</span>
          </div>
        </div>
      </PageSection>
    </styled.MediaOverviewIcons>
  );
};
