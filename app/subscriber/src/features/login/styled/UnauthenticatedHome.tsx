import styled from 'styled-components';

import { IUnauthenticatedHomeProps } from '..';

export const UnauthenticatedHome = styled.div<IUnauthenticatedHomeProps>`
  position: relative;
  overflow-x: auto;
  overflow-y: hidden;
  background: ${(props) => props.theme.css.bkPrimary};
  width: 100%;
  height: 100dvh;
  .footer {
    padding: 2%;
    text-align: left;
    position: absolute;
    border-radius: 0 0 0.5em 0.5em;
    bottom: 0;
    left: 0;
    background-color: ${(props) => props.theme.css.dialogBoxBkSecondary};
  }

  .containing-row {
    overflow-x: auto;
    overflow-y: hidden;
    height: auto;
  }

  .app-logo {
    padding: 1%;
    @media (min-width: 1450px) {
      margin-left: 5%;
    }
    @media (min-width: 850px) {
      max-width: 50em;
      max-height: 25em;
    }
    @media (max-width: 850px) {
      max-width: 30em;
      max-height: 15em;
    }
    max-width: 50em;
    max-height: 25em;

    @media (max-width: 1450px) {
      margin-left: auto;
      margin-right: auto;
    }
    margin-top: 2.5%;

    margin-bottom: 2.5%;
  }

  .copyright {
    font-size: 0.8em;
    margin-left: 5%;
  }

  a {
    margin-bottom: 2%;
  }

  .containing-box {
    padding: 2%;
    display: flex;
    max-height: fit-content;
    overflow: hidden;
    @media (max-width: 1550px) {
      display: inline-block;
      margin-left: 5em;
    }
  }

  .centered-login-box {
    @media (min-width: 1450px) {
      margin-left: auto;
      margin-right: auto;
    }
  }

  .system-message-containing-box {
    padding: 1%;
  }

  .system-message-box {
    display: inline-block;
    position: relative;
    margin-top: 2%;
    padding: 2%;
    height: 487px;
    float: left;
    background-color: ${(props) => props.theme.css.stickyNoteColor};
    margin-right: auto;
    margin-left: 20px;
    @media (max-width: 1450px) {
      min-width: 50em;
    }
    @media (max-width: 768px) {
      margin-bottom: 0.5em;
    }
    @media (min-width: 1450px) {
      max-width: 50em;
    }
    width: 50em;
  }
  .mm-logo {
    height: 8%;
    @media (max-width: 768px) {
      height: 6%;
      right: 1%;
    }
    position: fixed;
    right: 5%;
    bottom: 1%;
    z-index: 100;
  }
  .logo-row {
    justify-content: flex-end;
    img {
    }
  }

  .main-box {
    box-shadow: 0px 2px 6px #0000000a;
    background-color: #ffffff;
    max-height: fit-content;
    margin-right: 1em;
    overflow: hidden;
    width: 110em;
    @media (min-width: 850px) {
      max-width: 70em;
    }
    @media (max-width: 1550px) {
      min-width: 50em;
      margin-left: 1%;
      margin-bottom: 1%;
    }
    @media (min-width: 1450px) {
      max-width: 130em;
    }
    .top-bar-box {
      margin: 0;
      background-color: ${(props) => props.theme.css.dialogBoxBkPrimary};
      color: white;
      padding: 0.5em;
      font-size: 18px;
      font-weight: 400;
      line-height: 22px;
      letter-spacing: 0em;
      text-align: center;
    }
  }
`;
