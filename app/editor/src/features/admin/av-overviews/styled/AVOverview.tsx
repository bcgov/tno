import styled from 'styled-components';
import { FormPage } from 'tno-core';

export const AVOverview = styled(FormPage)`
  .page-header {
    margin-bottom: 1rem;
  }
  .title {
    font-size: 1.5rem;
  }
  .actions {
    margin-top: 1rem;
    .save {
      margin-left: auto;
    }
  }
  .new-section {
    margin-top: 1rem;
    .icon {
      align-self: center;
      margin-left: 0.5rem;
    }
  }

  @keyframes spin {
    0% {
      transform: rotate(0deg);
    }
    100% {
      transform: rotate(359deg);
    }
  }
  .buttons {
    /* .clear {
      background-color: #bc202e;
      border: none;
    }
    .new-story {
      background-color: #2e7d32;
      border: none;
    } */
    .save-items {
      margin-left: auto;
      .spinner {
        animation: spin 2s linear infinite;
      }
    }
    margin-left: auto;
    .icon {
      align-self: center;
      margin-left: 0.5rem;
    }
    .section {
      .section-title {
        font-size: 1.4rem;
      }
    }
  }
`;
