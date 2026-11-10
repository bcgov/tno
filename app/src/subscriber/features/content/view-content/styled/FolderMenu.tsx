import styled from 'styled-components';
import { Col } from 'tno-core';

export const FolderMenu = styled(Col)`
  .title-row {
    svg {
      color: ${(props) => props.theme.css.iconPrimaryColor};
      margin-right: 0.5em;
    }
    color: ${(props) => props.theme.css.fPrimaryColor};
    text-transform: uppercase;
    font-weight: bold;
  }
  input {
    font-size: 1em;
    border: none;
    border-bottom: 0.1rem solid;
    border-radius: 0;
    padding: 0;
    &:focus {
      box-shadow: none;
      outline: none;
    }
  }
  .add-folder {
    display: flex;
    background-color: transparent;
    text-transform: uppercase;
    color: ${(props) => props.theme.css.btnBkPrimary};
    &:hover {
      cursor: pointer;
    }
    &:disabled {
      cursor: not-allowed;
    }

    border: 0.1em solid ${(props) => props.theme.css.btnBkPrimary};
    height: 1.5em;
    margin-left: 0;
    min-width: fit-content;
  }
  background-color: white;
  .add-row {
    max-height: 2.5em;
    border-bottom: 0.1rem solid;
  }
  .folder-name {
    max-height: 1.5em;
    margin-top: auto;
    border: none;
    border-radius: 1;
    &:focus {
      outline: none;
    }
  }

  .folder-row {
    color: ${(props) => props.theme.css.fPrimaryColor};
    margin-top: 0.5em;
    &:hover {
      background-color: ${(props) => props.theme.css.highlightPrimary};
    }
    .popout-icon {
      margin-left: auto;
    }
  }
  .add-title {
    font-weight: bold;
    text-transform: uppercase;
    display: flex;
    align-items: center;
    color: ${(props) => props.theme.css.fPrimaryColor};
    margin-top: 0.5em;
    svg {
      margin-right: 0.5em;
      color: ${(props) => props.theme.css.iconPrimaryColor};
    }
  }
`;
