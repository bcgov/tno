import React from 'react';
import { useSearchParams } from 'react-router-dom';
import { Col, Row } from 'tno-core';

import * as styled from './styled';

/**
 * Component to submit access requests.
 * @returns Access request page.
 */
export const TranscriptionRequest: React.FC = () => {
  const [params] = useSearchParams();
  const requested = params.get('requested');
  return (
    <styled.TranscriptionRequest>
      <Col>
        <img alt="MMI Logo" className="app-logo" src="/assets/MMinsights_logo_dark_text.svg" />
        <Row className="containing-row">
          <Col className="main-box">
            <hr />
            <p className="top-bar-box">
              {!requested
                ? 'The transcript request has been submitted.'
                : 'The transcript has already been requested.'}
            </p>
            <div className={'containing-box'}>
              <Col className="message-box">
                You will receive an email to let you know when the transcript has been prepared and
                is ready to view.
              </Col>
            </div>
          </Col>
        </Row>
      </Col>
    </styled.TranscriptionRequest>
  );
};
