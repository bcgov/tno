import React from 'react';
import {
  FaFileAlt,
  FaFolderPlus,
  FaNewspaper,
  FaPrint,
  FaQuoteLeft,
  FaShareSquare,
} from 'react-icons/fa';
import { Tooltip } from 'react-tooltip';
import { IContentModel, ITagModel, Row } from 'tno-core';

import { ActionNames } from './constants';
import { FolderMenu } from './FolderMenu';
import * as styled from './styled';

export interface IViewContentToolbarProps {
  /** The current content that is being viewed. */
  tags: ITagModel[] | [];
  content: IContentModel;
}

/**
 * Shows the various actions to be presented on a piece of content.
 * @returns Toolbar for the ViewContent component
 */
export const ViewContentToolbar: React.FC<IViewContentToolbarProps> = ({ tags, content }) => {
  const [active, setActive] = React.useState<ActionNames>(ActionNames.ReadStory);

  return (
    <styled.ViewContentToolbar>
      <Row className="main-row">
        <Row alignItems="flex-end" style={{ display: 'flex' }}>
          <p className="actions-label">ACTIONS: </p>
          <div className="action-icons">
            <FaNewspaper
              data-tooltip-id="main-tooltip"
              data-tooltip-content={ActionNames.ReadStory}
              onClick={() => setActive(ActionNames.ReadStory)}
              className={active === ActionNames.ReadStory ? 'active' : ''}
            />
            <FaPrint
              data-tooltip-id="main-tooltip"
              data-tooltip-content={ActionNames.Print}
              onClick={() => setActive(ActionNames.Print)}
              className={active === ActionNames.Print ? 'active' : ''}
            />
            <FaQuoteLeft
              data-tooltip-id="main-tooltip"
              data-tooltip-content={ActionNames.Quotes}
              onClick={() => setActive(ActionNames.Quotes)}
              className={active === ActionNames.Quotes ? 'active' : ''}
            />
            <FaFolderPlus
              data-tooltip-id="folder"
              onClick={() => setActive(ActionNames.AddToFolder)}
              className={active === ActionNames.AddToFolder ? 'active' : ''}
            />
            <Tooltip
              style={{
                backgroundColor: '#FFFFCC',
                color: 'black',
                opacity: '1',
                boxShadow: '0 0 8px #464545',
              }}
              place="top"
              id="folder"
            >
              {ActionNames.AddToFolder}
            </Tooltip>
            <Tooltip
              clickable
              variant="light"
              className="folder-menu"
              place="bottom"
              openOnClick
              style={{ opacity: '1', boxShadow: '0 0 8px #464545', zIndex: '999' }}
              id="folder"
            >
              <FolderMenu content={[{ ...content, sortOrder: 0, contentId: content.id }]} />
            </Tooltip>
            <FaFileAlt
              data-tooltip-id="main-tooltip"
              data-tooltip-content={ActionNames.AddToReport}
              onClick={() => setActive(ActionNames.AddToReport)}
              className={active === ActionNames.AddToReport ? 'active' : ''}
            />
            <FaShareSquare
              data-tooltip-id="main-tooltip"
              data-tooltip-content={ActionNames.Share}
              onClick={() => setActive(ActionNames.Share)}
              className={active === ActionNames.Share ? 'active' : ''}
            />
          </div>
        </Row>
      </Row>
      <Row className="hrz-line" />
    </styled.ViewContentToolbar>
  );
};
