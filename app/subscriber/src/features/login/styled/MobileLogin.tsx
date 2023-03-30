import styled from 'styled-components';

export const MobileLogin = styled.div`
  .mobile-view {
    .login-content {
      display: flex;
    }
    .mobile-title {
      min-width: 100%;
    }
    .top-bar-box {
      background-color: #221f1f;
      color: white;
      padding: 0.5em;
    }
    .buttons {
      margin-top: 13.5%;
    }
    .containing-box {
      background-color: white;
    }

    button {
      border-radius: 1.25rem;
      min-width: 5rem;
      border: none;
      display: flex;
      justify-content: center;
      &.red {
        background-color: #bc202e !important;
      }
      &.cyan {
        background-color: #24b6d4 !important;
      }
    }
  }
`;
