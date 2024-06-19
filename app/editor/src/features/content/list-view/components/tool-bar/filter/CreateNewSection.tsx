import { NavigateOptions, useTab } from 'components/tab-control';
import React from 'react';
import { FaFileAlt, FaFileAudio, FaFileImage, FaFileInvoice } from 'react-icons/fa';
import { GiFairyWand } from 'react-icons/gi';
import { ContentTypeName, Row } from 'tno-core';

import * as styled from './styled';

export interface ICreateNewSectionProps {
  /** The title of the section */
  title?: string;
  /** What types of content will be displayed. */
  contentTypes?: ContentTypeName[];
}

/**
 * Section containing the create new content buttons
 * @returns Section with three separate create content buttons
 */
export const CreateNewSection: React.FC<ICreateNewSectionProps> = ({
  title,
  contentTypes = [
    ContentTypeName.AudioVideo,
    ContentTypeName.PrintContent,
    ContentTypeName.Image,
    ContentTypeName.Internet,
  ],
}) => {
  const { navigate } = useTab();

  const getIcon = React.useCallback(
    (contentType: ContentTypeName) => {
      switch (contentType) {
        case ContentTypeName.AudioVideo:
          return (
            <FaFileAudio
              key={contentType}
              data-tooltip-content="Radio/TV"
              data-tooltip-id="main-tooltip"
              onClick={(e) =>
                e.ctrlKey
                  ? navigate(0, '/contents', NavigateOptions.NewTab)
                  : navigate(0, '/contents')
              }
              className="action-button"
            />
          );
        case ContentTypeName.PrintContent:
          return (
            <FaFileAlt
              key={contentType}
              data-tooltip-content="Print content"
              data-tooltip-id="main-tooltip"
              onClick={(e) =>
                e.ctrlKey ? navigate(0, '/papers', NavigateOptions.NewTab) : navigate(0, '/papers')
              }
              className="action-button"
            />
          );
        case ContentTypeName.Image:
          return (
            <FaFileImage
              key={contentType}
              data-tooltip-content="Image"
              data-tooltip-id="main-tooltip"
              onClick={(e) =>
                e.ctrlKey ? navigate(0, '/images', NavigateOptions.NewTab) : navigate(0, '/images')
              }
              className="action-button"
            />
          );
        case ContentTypeName.Internet:
          return (
            <FaFileInvoice
              key={contentType}
              data-tooltip-content="Online"
              data-tooltip-id="main-tooltip"
              onClick={(e) =>
                e.ctrlKey
                  ? navigate(0, '/stories', NavigateOptions.NewTab)
                  : navigate(0, '/stories')
              }
              className="action-button"
            />
          );
      }
    },
    [navigate],
  );

  return (
    <styled.CreateNewSection title={title} label="CREATE SNIPPET" icon={<GiFairyWand />}>
      <Row className="create-new">{contentTypes.map((ct) => getIcon(ct))}</Row>
    </styled.CreateNewSection>
  );
};
