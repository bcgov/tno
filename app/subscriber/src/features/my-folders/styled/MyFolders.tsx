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
    width: 60%;
  }
  .create-new {
    .wand {
      margin-top: auto;
      margin-right: 0.25em;
      margin-bottom: auto;
    }
    width: 50%;
    .create-text {
      font-size: 1rem;
      font-weight: 800;
      align-self: center;
    }
    margin-left: auto;
    margin-right: auto;
    margin-bottom: 0.5em;
  }
  .create-button {
    height: 2.5em;
    width: 2.5em;
    display: flex;
    border: none;
    &:focus {
      outline: none;
    }
    cursor: pointer;
    border-radius: 0.5em;
    svg {
      display: flex;
      align-self: center;
      margin-right: auto;
      margin-left: auto;
    }
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
