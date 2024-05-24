import { FormPage } from 'components/formpage';
import styled from 'styled-components';

export const AVOverview = styled(FormPage)`
  h1 {
    font-size: 1.5rem;
    text-transform: unset;
  }

  .page-header {
    margin-bottom: 1rem;
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
    margin-left: auto;
    display: flex;
    gap: 0.5rem;

    .save-items {
      margin-left: auto;
      .spinner {
        animation: spin 2s linear infinite;
      }
    }

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
