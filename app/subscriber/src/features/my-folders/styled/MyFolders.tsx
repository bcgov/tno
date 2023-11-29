import styled from 'styled-components';
import { Col } from 'tno-core';

export const MyFolders = styled(Col)`
  max-height: calc(100vh - 6.5em);
  .react-tooltip {
    z-index: 999;
  }
  .options {
    box-shadow: 0 0 0.5rem #c7c7c7;
    opacity: 1;
    padding-left: 1.5em;
    padding-right: 1.5em;
    .option {
      &:hover {
        text-decoration: underline;
        color: ${(props) => props.theme.css.sidebarIconHoverColor};
        cursor: pointer;
      }
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
  /* table styling */
  .table {
    width: 100%;
    overflow: hidden;
    .re-name {
      height: 1.5em;
      padding: 0;
      &:focus {
        box-shadow: none;
      }
    }
    .row {
      &:hover {
        cursor: pointer;
      }
    }
    .header {
      background-color: #f5f6fa;
      font-family: 'Roboto', sans-serif;
      font-size: 0.8em;
      /* box shadow only on bottom */
      box-shadow: 0 2px 4px 0 rgba(0, 0, 0, 0.1);
      border: none;
      color: #7c7e8a;

      .column {
        background-color: #f5f6fa;
      }
    }
    .elips {
      &:hover {
        color: ${(props) => props.theme.css.sidebarIconHoverColor};
        transform: scale(1.1);
      }
    }
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
`;
