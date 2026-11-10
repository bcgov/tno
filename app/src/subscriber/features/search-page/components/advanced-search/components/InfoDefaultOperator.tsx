import { FaInfoCircle } from 'react-icons/fa';
import { Col, Row } from 'tno-core';

import { InfoTooltip } from './InfoTooltip';
import * as styled from './styled';

/** Component that displays help text for elastic search queries */
export const InfoDefaultOperator: React.FC = () => {
  return (
    <styled.InfoDefaultOperator>
      <FaInfoCircle data-tooltip-id="toggle-info-default-operator" className="info-icon" />
      <InfoTooltip id="toggle-info-default-operator">
        <Col>
          <div>
            <FaInfoCircle className="info-icon" />
            <span>
              <b>Default operator</b>
            </span>
            <Row>
              <div> </div>
            </Row>
            <Row className="row">Select how your query should treat spaces between keywords.</Row>
            <Row className="row">Examples:</Row>
            <Row className="row">
              <span>
                <b>When "OR" is selected,</b>
              </span>
            </Row>
            <Row className="row">
              <span>
                This query:&nbsp;
                <b>cat dog</b>
              </span>
            </Row>
            <Row className="row">
              <span>
                Will return results that contain &nbsp;<i>either</i>&nbsp; cat OR dog
              </span>
            </Row>
            <Row>
              <div>
                <span>&nbsp;</span>
              </div>
            </Row>
            <Row className="row">
              <span>
                <b>When "AND" is selected,</b>
              </span>
            </Row>
            <Row className="row">
              <span>
                This query:&nbsp;<b>cat dog</b>
              </span>
            </Row>
            <Row className="row">
              <span>
                Will only return results that contain &nbsp;<i>both</i>&nbsp; cat AND dog
              </span>
            </Row>
          </div>
        </Col>
      </InfoTooltip>
    </styled.InfoDefaultOperator>
  );
};
