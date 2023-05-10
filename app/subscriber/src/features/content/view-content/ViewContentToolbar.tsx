import { Tags } from 'components/form/tags';
import React from 'react';
import {
  FaFileAlt,
  FaFolderPlus,
  FaNewspaper,
  FaPrint,
  FaQuoteLeft,
  FaShareSquare,
} from 'react-icons/fa';
import { Row } from 'tno-core';

import { ActionNames } from './constants';
import * as styled from './styled';

/**
 * Shows the various actions to be presented on a piece of content.
 * @returns Toolbar for the ViewContent component
 */
export const ViewContentToolbar: React.FC = () => {
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
              data-tooltip-id="main-tooltip"
              data-tooltip-content={ActionNames.AddToFolder}
              onClick={() => setActive(ActionNames.AddToFolder)}
              className={active === ActionNames.AddToFolder ? 'active' : ''}
            />
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
        <Tags />
      </Row>
      <Row className="hrz-line" />
    </styled.ViewContentToolbar>
  );
};
