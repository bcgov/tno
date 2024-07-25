import React from 'react';
import { Col, Row } from 'tno-core';

import { IDPOptions } from './IDPOptions';
import * as styled from './styled';

export interface IMobileLoginProps {
  login: (hint?: string) => void;
}

/**
 * Mobile login is the component that will be viewed when an unauthenticated user visits the site from a mobile device.
 * @param login The function to be called when the user clicks on the login button.
 * @returns MobileLogin component.
 */
export const MobileLogin: React.FC<IMobileLoginProps> = ({ login }) => {
  return (
    <styled.MobileLogin>
      <Col className="mobile-view">
        <Row className="mobile-title">
          <div className="app-logo"></div>
        </Row>
        <IDPOptions login={login}>
          <br />
          <p>
            <a href="mailto:Scott.Ryckman@gov.bc.ca">Learn more about obtaining a subscription</a>
          </p>
        </IDPOptions>
      </Col>
    </styled.MobileLogin>
  );
};
