import { PageSection } from 'components/section';
import React from 'react';
import { FaScroll, FaVideo } from 'react-icons/fa6';

import * as styled from './styled';

export const MediaOverviewIcons: React.FC = () => {
  return (
    <styled.MediaOverviewIcons>
      <PageSection header="Media Overview Icons">
        <div className="content">
          <div>
            <FaScroll className="scroll-icon" />
            <span>Story summaries appearing in blue feature a transcript. Click icon to view.</span>
          </div>
          <div>
            <FaVideo className="video-icon" />
            <span>Includes a video. Click icon to view.</span>
          </div>
        </div>
      </PageSection>
    </styled.MediaOverviewIcons>
  );
};
