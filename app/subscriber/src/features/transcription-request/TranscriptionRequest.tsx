import React from 'react';
import { Col, Row } from 'tno-core';

import * as styled from './styled';

/**
 * Component to submit access requests.
 * @returns Access request page.
 */
export const TranscriptionRequest: React.FC = () => {
  return (
    <styled.TranscriptionRequest>
      <Col>
        <img alt="MMI Logo" className="app-logo" src="/assets/MMinsights_logo_dark_text.svg" />
        <Row className="containing-row">
          <Col className="main-box">
            <p className="top-bar-box">Transcription Requested</p>
            <div className={'containing-box'}>
              <Col className="message-box">
                <h1>Please wait while your user information is being set up.</h1>
                <h2>You will be redirected automatically.</h2>
              </Col>
            </div>
          </Col>
        </Row>
      </Col>
    </styled.TranscriptionRequest>
  );
};
