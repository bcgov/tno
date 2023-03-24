import { Button, Col, Row } from 'tno-core';
import * as styled from './styled';

export interface IMobileLoginProps {
  login: () => void;
}

/**
 * TextBox provides a customizable container to place informative information in.
 * @param width Div width element attribute.
 * @param backgroundColor Div background-color element attribute.
 * @returns LogoPanel component.
 */
export const MobileLogin: React.FC<IMobileLoginProps> = ({ login }) => {
  return (
    <styled.MobileLogin>
      <Col className="mobile-view">
        <Row className="mobile-title">
          <img alt="MMIA Logo" src="/assets/mminsights_logo_black.svg" />
        </Row>
        <Row>
          <p className="top-bar-box">
            Media Monitoring is a paid service offered through the BC Government that allows
            subscribers to see British Columbia’s news at a glance.
          </p>
        </Row>
        <Row>
          <div className="containing-box">
            <b>Key feautres: </b>
            <ul>
              <li>Aggregation of newspapers, radio shows, and online articles </li>
              <li>Transcription services</li>
              <li>BC’s top stories as they break</li>
              <li>Articles related to major stories</li>
            </ul>
            <a href="www.google.ca">Learn more about obtaining a subscription... </a>
            <Col className="login-box">
              <b>If you have a subscription, login here: </b>
              <div className="login-content">
                <div className="buttons">
                  <Button className="red" onClick={() => login()}>
                    IDIR
                  </Button>
                  <Button className="cyan">BCeID</Button>
                </div>
                <Row className="copyright">
                  <b>Copyright info:</b>
                  <p>
                    This account grants you access to copyrighted material for your own use. It does
                    not grant you permission to fix, copy, reproduce or archive any of the material
                    contained within. <br /> <br />
                    You cannot redistribute this information to anyone without violating your
                    copyright agreement.
                  </p>
                </Row>
              </div>
            </Col>
          </div>
        </Row>
      </Col>
    </styled.MobileLogin>
  );
};
