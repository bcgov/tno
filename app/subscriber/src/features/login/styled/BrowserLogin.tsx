import styled from 'styled-components';

export const BrowserLogin = styled.div`
  .containing-row {
    margin-left: auto;
    margin-right: auto;
  }

  .login-content {
    margin-top: 2%;
    display: flex;
    justify-content: space-between;
  }

  .login-box {
    margin-top: 2%;
    border: 2px solid black;
    border-radius: 0.5rem;
    padding: 2%;
  }

  .buttons {
    margin-top: 1.5%;
  }

  button {
    border-radius: 1.25rem;
    margin-bottom: 10%;
    min-width: 5rem;
    border: none;
    display: flex;
    justify-content: center;
    color: white;
    &.red {
      background-color: #bc202e;
      &:hover {
        background-color: #a61c29;
      }
    }
    &.cyan {
      background-color: #24b6d4;
      &:hover {
        background-color: #1d9dbf;
      }
    }
  }
`;
