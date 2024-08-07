import { LightweightModal } from 'components/lightweight-modal/LightweightModal';
import React from 'react';

import { IDPOptions } from './IDPOptions';
import * as styled from './styled';
import { SystemMessage } from './SystemMessage';

export interface IAppLoginProps {
  login: Function;
}

export const AppLogin: React.FC<IAppLoginProps> = ({ login }) => {
  const [showModal, setShowModal] = React.useState(false);
  return (
    <styled.AppLogin>
      <img alt="MMI Logo" className="app-logo" src="/assets/MMinsights_logo_dark_text.svg" />
      <div className="login-box">
        <div className="top-bar">
          Media Monitoring is a paid service offered through the BC Government that allows
          subscribers to see British Columbia’s news at a glance.
        </div>
        <SystemMessage />
        <IDPOptions login={login}>
          <div className="learn-more" onClick={() => setShowModal(true)}>
            Learn more about obtaining a subscription...
          </div>
        </IDPOptions>
      </div>
      <LightweightModal open={showModal} close={() => setShowModal(false)}>
        <div className="modal-content">
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
                <a href="mailto:Scott.Ryckman@gov.bc.ca">Scott.Ryckman@gov.bc.ca</a> to get set up.
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
      </LightweightModal>
    </styled.AppLogin>
  );
};
