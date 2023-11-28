import React from 'react';
import { FaShield } from 'react-icons/fa6';
import { Tooltip } from 'react-tooltip';
import { Col } from 'tno-core';

import * as styled from './styled';

export const InfoShield: React.FC = () => {
  return (
    <styled.InfoShield className="info-shield">
      <FaShield data-tooltip-id="info-shield" />
      <Tooltip clickable className="react-tooltip" id="info-shield" place="top" delayHide={800}>
        <Col>
          <a
            href="https://www2.gov.bc.ca/gov/content?id=79F93E018712422FBC8E674A67A70535"
            target="_blank"
            className="tooltip-link"
            rel="noreferrer"
          >
            Disclaimer
          </a>
          <a
            href="https://www2.gov.bc.ca/gov/content?id=9E890E16955E4FF4BF3B0E07B4722932"
            target="_blank"
            className="tooltip-link"
            rel="noreferrer"
          >
            Privacy
          </a>
          <a
            href="https://www2.gov.bc.ca/gov/content?id=E08E79740F9C41B9B0C484685CC5E412"
            target="_blank"
            className="tooltip-link"
            rel="noreferrer"
          >
            Accessibility
          </a>
          <a
            href="https://www2.gov.bc.ca/gov/content?id=1AAACC9C65754E4D89A118B875E0FBDA"
            target="_blank"
            className="tooltip-link"
            rel="noreferrer"
          >
            Copyright
          </a>
          <a
            href="https://www2.gov.bc.ca/gov/content?id=6A77C17D0CCB48F897F8598CCC019111"
            className="tooltip-link"
            target="_blank"
            rel="noreferrer"
          >
            Contact Us
          </a>
        </Col>
      </Tooltip>
    </styled.InfoShield>
  );
};
