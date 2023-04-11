import styled from 'styled-components';
import { Row } from 'tno-core';

export const SubscriberFooter = styled(Row)`
  /* save realestate for smaller devices - 768 size of iPad mini */
  @media (max-width: 767px) {
    font-size: 0.75em;
  }
  /* position bottom of screen */
  position: fixed;
  bottom: 0;
  width: 100%;

  background-color: #ddd6c8;
  align-self: flex-end;
  min-height: 2em;
  box-shadow: 0 0.5em 0.5em -0.4em rgba(0, 0, 0, 0.2), 0 0 0 1px rgba(0, 0, 0, 0.02);
  .contents {
    @media (max-width: 500px) {
      margin-left: 0.25em;
    }
    @media (min-width: 500px) {
      margin-left: 1em;
    }
  }
  a {
    align-self: center;
    color: #5c5954;
    @media (max-width: 500px) {
      margin-right: 0.5em;
    }
    @media (min-width: 500px) {
      margin-right: 1em;
    }
  }

  .mm-logo {
    max-height: 50%;
    max-width: fit-content;
    position: absolute;
  }
`;
