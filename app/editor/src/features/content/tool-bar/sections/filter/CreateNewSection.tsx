import React from 'react';
import { FaFileAlt, FaFileAudio, FaFileImage, FaFileInvoice } from 'react-icons/fa';
import { GiFairyWand } from 'react-icons/gi';
import { useNavigate } from 'react-router-dom';
import { ContentTypeName, Row } from 'tno-core';

import * as styled from './styled';

export interface ICreateNewSectionProps {
  /** What types of content will be displayed. */
  contentTypes?: ContentTypeName[];
}

/**
 * Section containing the create new content buttons
 * @returns Section with three separate create content buttons
 */
export const CreateNewSection: React.FC<ICreateNewSectionProps> = ({
  contentTypes = [
    ContentTypeName.Snippet,
    ContentTypeName.PrintContent,
    ContentTypeName.Image,
    ContentTypeName.Story,
  ],
}) => {
  const navigate = useNavigate();

  const getIcon = React.useCallback(
    (contentType: ContentTypeName) => {
      switch (contentType) {
        case ContentTypeName.Snippet:
          return (
            <FaFileAudio
              key={contentType}
              data-tooltip-content="Radio/TV"
              data-tooltip-id="main-tooltip"
              onClick={() => navigate('/snippets/0')}
              className="action-button"
            />
          );
        case ContentTypeName.PrintContent:
          return (
            <FaFileAlt
              key={contentType}
              data-tooltip-content="Print content"
              data-tooltip-id="main-tooltip"
              onClick={() => navigate('/papers/0')}
              className="action-button"
            />
          );
        case ContentTypeName.Image:
          return (
            <FaFileImage
              key={contentType}
              data-tooltip-content="Image"
              data-tooltip-id="main-tooltip"
              onClick={() => navigate('/images/0')}
              className="action-button"
            />
          );
        case ContentTypeName.Story:
          return (
            <FaFileInvoice
              key={contentType}
              data-tooltip-content="Internet"
              data-tooltip-id="main-tooltip"
              onClick={() => navigate('/stories/0')}
              className="action-button"
            />
          );
      }
    },
    [navigate],
  );

  return (
    <styled.CreateNewSection label="CREATE SNIPPET" icon={<GiFairyWand />}>
      <Row className="create-new">{contentTypes.map((ct) => getIcon(ct))}</Row>
    </styled.CreateNewSection>
  );
};
