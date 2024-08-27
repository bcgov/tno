import styled from 'styled-components';
import { Col } from 'tno-core';

export const MyAccountSettings = styled(Col)`
  padding: 0.6em;

  .header-row {
    display: flex;
    align-items: center;
    font-size: 1rem;
    color: #333;
    margin-top: 0.5rem;
    margin-bottom: 0.25rem;
    margin-left: 0.75rem;
    border-bottom: 1px solid #ccc;

    .icon {
      font-size: 1.2rem;
      margin-right: 0.75rem;
    }

    .header-text {
      font-weight: bold;
      font-size: 1.1rem;
    }
  }

  .description {
    margin-left: 2.8rem;
  }

  .toggleContainer {
    display: flex;
    align-items: center;
    margin-left: 3rem;

    .vacation-mode-label {
      color: #008000;
      display: flex;
      align-items: center;
    }

    .icon {
      margin-right: 5px;
    }
  }
`;
