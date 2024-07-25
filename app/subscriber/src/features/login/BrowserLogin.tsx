import React from 'react';
import { Col, Show } from 'tno-core';

import { IDPOptions } from './IDPOptions';
import * as styled from './styled';

export interface IBrowserLoginProps {
  login: (hint?: string) => void;
}

/**
 * Browser login is the component that will be viewed when an unauthenticated user visits the site from a non mobile device.
 * @param login The function to be called when the user clicks on the login button.
 * @returns BrowserLogin component.
 */
export const BrowserLogin: React.FC<IBrowserLoginProps> = ({ login }) => {
  const [showModal, setShowModal] = React.useState(false);

  return (
    <styled.BrowserLogin>
      <Col>
        <img alt="MMI Logo" className="app-logo" src="/assets/MMinsights_logo_dark_text.svg" />
        <IDPOptions login={login}>
          <br />
          <span
            className="modalOpen"
            onClick={() => {
              setShowModal(true);
            }}
          >
            Learn more about obtaining a subscription...
          </span>
        </IDPOptions>
      </Col>
      <Show visible={showModal}>
        <div id="myModal" className="modal">
          <div className="modal-content">
            <span
              className="close"
              onClick={() => {
                setShowModal(false);
              }}
            >
              &times;
            </span>
            <div>
              <p>
                <b>Thank you for your interest in Media Monitoring Insights.</b>
              </p>
              <p>
                <b>Key features:</b>
              </p>
              <ul>
                <li>Aggregation of newspapers, radio shows, and online articles</li>
                <li>Transcription services</li>
                <li>BC’s top stories as they break</li>
                <li>Articles related to major stories</li>
              </ul>
              <p>
                Access to this service is limited to Government of British Columbia staff as per our
                agreements with our media suppliers.
              </p>
              <br />
              <p>
                <b>
                  If you are eligible for access, email{' '}
                  <a href="mailto:Scott.Ryckman@gov.bc.ca">Scott.Ryckman@gov.bc.ca</a> to get set
                  up.
                </b>
              </p>
              <br />
              <p>
                Note: <br />
                There is a cost recovery charge in excess of $4,500 annually, please ensure that you
                have authorization for this purchase prior to emailing.
              </p>
            </div>
          </div>
        </div>
      </Show>
    </styled.BrowserLogin>
  );
};
