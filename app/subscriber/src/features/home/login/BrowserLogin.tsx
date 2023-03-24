import { Button, Col, Row } from 'tno-core';
import * as styled from './styled';

export interface IBrowserLoginProps {
  login: () => void;
}

/**
 * TextBox provides a customizable container to place informative information in.
 * @param width Div width element attribute.
 * @param backgroundColor Div background-color element attribute.
 * @returns LogoPanel component.
 */
export const BrowserLogin: React.FC<IBrowserLoginProps> = ({ login }) => {
  return (
    <styled.BrowserLogin>
      <Row>
        <img alt="MMIA Logo" className="app-logo" src="/assets/mminsights_logo_black.svg" />

        <Row className="containing-row">
          <Col className="main-box">
            <p className="top-bar-box">
              Media Monitoring is a paid service offered through the BC Government that allows
              subscribers to see British Columbia’s news at a glance.
            </p>
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
                  <div className="copyright">
                    <b>Copyright info:</b>
                    <p>
                      This account grants you access to copyrighted material for your own use. It
                      does not grant you permission to fix, copy, reproduce or archive any of the
                      material contained within. <br /> <br />
                      You cannot redistribute this information to anyone without violating your
                      copyright agreement.
                    </p>
                  </div>
                </div>
              </Col>
            </div>
          </Col>
          <Col className="alert-box">
            <div className="alert-containing-box">
              <b>System Notices</b>
              <br />
              <p>
                Service interruption Dec 29 - Jan 3. Stories will not be updated over the holidays.{' '}
                <br />
                <br />
                Jan 4th - Updates resume, business as usual.
              </p>
            </div>
          </Col>
        </Row>
      </Row>

      <img src="/assets/mm_logo.svg" alt="MM Logo" className="mm-logo" />
    </styled.BrowserLogin>
  );
};
