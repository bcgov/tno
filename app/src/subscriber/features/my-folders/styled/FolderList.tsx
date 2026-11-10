import styled from 'styled-components';
import { Col } from 'tno-core';

export const FolderList = styled(Col)`
  width: 100%;
  font-size: 1.15rem;
  svg {
    align-self: center;
    color: ${(props) => props.theme.css.iconPrimaryColor};
  }
  padding: 0.5rem;
  .folder-row {
    padding: 0.25rem;
    align-items: center;
    &.active {
      border-left: 0.35rem solid ${(props) => props.theme.css.highlightActive};
    }
    cursor: pointer;
    &:hover {
      background-color: ${(props) => props.theme.css.highlightPrimary};
    }
  }
  font-weight: bold;
  color: ${(props) => props.theme.css.btnBkPrimary};
  .story-count {
    margin-left: auto;
  }
  .settings {
    cursor: pointer;
    margin-left: 0.5rem;
    &:hover {
      transform: scale(1.1);
    }
  }
`;
