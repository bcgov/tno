import styled from 'styled-components';

import { IUnauthenticatedHomeProps } from '..';

export const UnauthenticatedHome = styled.div<IUnauthenticatedHomeProps>`
  position: relative;
  overflow: hidden;
  min-height: calc(100vh - 1.5em);
  background-color: ${(props) => props.theme.css.beigeBackgroundColor};
  width: 100%;

  .app-logo {
    margin-left: 5%;
    margin-top: 2.5%;
    max-width: 30%;
    max-height: 15%;
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
    max-width: fit-content;
    max-width: 50%;
  }
  .mm-logo {
    height: 8%;
    position: fixed;
    right: 5%;
    bottom: 1%;
  }
  .logo-row {
    justify-content: flex-end;
    img {
    }
  }

  .main-box {
    box-shadow: 0px 2px 6px #0000000a;
    background-color: #ffffff;
    max-width: 50%;
    max-height: fit-content;
    .top-bar-box {
      margin: 0;
      background-color: black;
      color: white;
      padding: 1%;
    }
  }
`;
