import { Tags } from 'components/form/tags';
import {
  FaFileAlt,
  FaFolderPlus,
  FaNewspaper,
  FaPrint,
  FaQuoteLeft,
  FaShareSquare,
} from 'react-icons/fa';
import { Row } from 'tno-core';

import * as styled from './styled';

/**
 * Shows the various actions to be presented on a piece of content.
 * @returns Toolbar for the ViewContent component
 */
export const ViewContentToolbar: React.FC = () => {
  return (
    <styled.ViewContentToolbar>
      <Row className="main-row">
        <Row alignItems="flex-end" style={{ display: 'flex' }}>
          <p className="actions-label">ACTIONS: </p>
          <div className="action-icons">
            <FaNewspaper />
            <FaPrint />
            <FaQuoteLeft />
            <FaFolderPlus />
            <FaFileAlt />
            <FaShareSquare />
          </div>
        </Row>
        <Tags />
      </Row>
      <Row className="hrz-line" />
    </styled.ViewContentToolbar>
  );
};
