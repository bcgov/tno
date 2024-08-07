import styled from 'styled-components';
import { Col } from 'tno-core';

export const AppLogin = styled(Col)`
  height: 100dvh;
  overflow-y: auto;
  .app-logo {
    padding-left: 2.5%;
    padding-top: 2.5%;
    width: fit-content;
    @media (max-width: 768px) {
      height: fit-content;
      width: 25em;
    }
  }
  .login-box {
    border: 0.25px solid ${(props) => props.theme.css.dialogBoxBkSecondary};
    .login-info {
      margin-left: auto;
      margin-right: auto;
      margin-bottom: 1em;
    }
    .button-box {
      margin-top: 1em;
      .idir-logo {
        background: url('/assets/Logos_IDIR.svg') no-repeat;
        border: 1px solid ${(props) => props.theme.css.btnBkPrimary};
        background-size: 100% 100%;
      }
    }
    .learn-more {
      color: ${(props) => props.theme.css.fRedColor};
      float: center;
      width: fit-content;
      margin-left: auto;
      margin-right: auto;
      margin-top: 3em;
      &:hover {v
        cursor: pointer;
      }
    }
    button {
      margin-left: auto;
      margin-right: auto;
      width: 7.5em;
      height: 3em;
      margin-bottom: 0.5em;
    }
    border-radius: 0.5em;
    max-width: 75em;
    margin-left: auto;
    margin-right: auto;
    @media (max-width: 768px) {
      margin-top: 0;
    }
    @media (min-width: 768px) {
      margin-top: 10em;
    }
    .top-bar {
      padding: 0.75em;
      border-radius: 0.5em 0.5em 0 0;
      background-color: ${(props) => props.theme.css.darkHeaderColor};
      font-size: 1.25em;
      color: ${(props) => props.theme.css.fPrimaryInvertColor};
    }
  }
`;
