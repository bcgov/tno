import styled from 'styled-components';
import { Col } from 'tno-core';

export const FolderMenu = styled(Col)`
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
  .add-folder {
    align-self: center;
    height: 1.5em;
    margin-left: 0;
    min-width: 1.5em;
  }

  .popout-icon {
    height: 1.5em;
    width: 1.5em;
    color: #a5a4bf;
    &:hover {
      transform: scale(1.1);
      cursor: pointer;
      color: ${(props) => props.theme.css.sideBarIconHoverColor};
    }
  }
  .folder-row {
    margin-top: 0.5em;
    &:hover {
      background-color: ${(props) => props.theme.css.searchItemHover};
    }
    .popout-icon {
      margin-left: auto;
    }
  }
`;
