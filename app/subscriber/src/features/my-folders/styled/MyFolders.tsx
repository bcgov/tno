import styled from 'styled-components';
import { Col } from 'tno-core';

export const MyFolders = styled(Col)`
  max-height: calc(100vh - 6.5em);
  /* option in tooltip */
  .option {
    &:hover {
      cursor: pointer;
      text-decoration: underline;
    }
  }
  .folder-name {
    height: 1.5em;
    margin-left: 0.5em;
  }
  .create-new {
    .create-text {
      font-size: 0.8em;
      align-self: center;
    }
    margin-left: auto;
    margin-bottom: 0.5em;
  }
  .create-button {
    &:focus {
      outline: none;
    }
    cursor: pointer;
    svg {
      align-self: center;
    }
    border-radius: 0.5em;
    height: 2rem;
    margin-top: 0.15em;
    width: 2rem;
    background-color: ${(props) => props.theme.css.btnBkPrimary};
    color: ${(props) => props.theme.css.btnPrimaryColor};
    padding-bottom: 0.25em;
    &:hover {
      transform: scale(1.1);
      color: ${(props) => props.theme.css.sidebarIconHoverColor};
    }
  }

  .re-name {
    input {
      outline: none;
      border: none;
      box-shadow: none;
      border-bottom: 1px solid ${(props) => props.theme.css.btnPkPrimary};
      border-radius: 0;
      font-size: 1em;
      padding: 0;
    }
  }
`;
