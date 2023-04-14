import styled from 'styled-components';

import { IUnauthenticatedHomeProps } from '..';

export const UnauthenticatedHome = styled.div<IUnauthenticatedHomeProps>`
  position: relative;
  overflow-y: auto;
  overflow-x: hidden;
  min-height: calc(100vh - 1.5em);
  background-color: ${(props) => props.theme.css.beigeBackgroundColor};
  width: 100%;

  .app-logo {
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
  }

  .alert-containing-box {
    padding: 1%;
  }

  .alert-box {
    b {
      color: red;
    }
    background-color: ${(props) => props.theme.css.stickyNoteColor};
    border: 1px solid ${(props) => props.theme.css.lightBlue};
    align-self: center;
    @media (max-width: 1450px) {
      min-width: 98%;
      margin-left: 1%;
    }
    @media (min-width: 1450px) {
      max-width: 50em;
    }
  }
  .mm-logo {
    height: 8%;
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
    @media (max-width: 1450px) {
      min-width: 98%;
      margin-left: 1%;
      margin-bottom: 1%;
    }
    @media (min-width: 1450px) {
      max-width: 50em;
    }
    max-height: fit-content;
    @media (min-width: 1450px) {
      margin-right: 1em;
    }
    margin-right: 1em;
    .top-bar-box {
      margin: 0;
      background-color: black;
      color: white;
      padding: 1%;
    }
  }
`;
