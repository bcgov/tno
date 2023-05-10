import styled from 'styled-components';
import { Col } from 'tno-core';

export const ViewContentToolbar = styled(Col)`
  max-height: 10em;
  width: 100%;
  .main-row {
    display: flex;
    /* number found manually for smoothness of resize */
    @media (max-width: 1580px) {
      justify-content: center;
    }
    @media (min-width: 1580px) {
      justify-content: space-between;
    }
  }
  .hrz-line {
    background-color: #c7c7c7;
    height: 0.1rem;
  }
  .actions-label {
    color: #5c5954;
  }
  .action-icons {
    svg {
      &:focus {
        outline: none;
      }
      cursor: pointer;
      align-self: center;
      margin-left: 0.5rem;
      height: 1.85rem;
      width: 2.5rem;
      color: #a5a4bf;
      padding-bottom: 0.25em;
      &:hover {
        transform: scale(1.1);
        color: #da6d78;
      }
    }
    align-content: center;
    .active {
      /* blue underline */
      border-bottom: 0.2rem solid #24b6d4;
      color: #43425d;
    }
  }
`;
