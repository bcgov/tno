import { FaCaretSquareDown, FaCheckSquare, FaInfoCircle } from 'react-icons/fa';
import { Col, Row } from 'tno-core';

import { InfoTooltip } from './InfoTooltip';
import * as styled from './styled';

/** Component that displays help text for elastic search queries */
export const ToggleFilterStyleInfo: React.FC = () => {
  return (
    <styled.ToggleFilterStyleInfo>
      <FaInfoCircle data-tooltip-id="toggle-filter-style-info" className="info-icon" />
      <InfoTooltip id="toggle-filter-style-info">
        <Col>
          <div>
            Switch between modes for display filter criteris:
            <ul>
              <Row className="row">
                <FaCheckSquare /> Display each filter option item as a checkbox. Usefule when
                selecting a large number of items.
              </Row>
              <Row className="row">
                <FaCaretSquareDown /> Show filter option items in a select drop down. Faster when
                search by name.
              </Row>
            </ul>
          </div>
        </Col>
      </InfoTooltip>
    </styled.ToggleFilterStyleInfo>
  );
};
