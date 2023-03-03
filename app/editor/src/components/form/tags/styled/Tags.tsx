import styled from 'styled-components';
import { Row } from 'tno-core';

export const Tags = styled(Row)`
  align-items: center;
  label {
    font-weight: bold;
  }
  .react-select__menu-list {
    background-color: red;
  }
  .react-draggable {
    z-index: 999;
    position: absolute;
    left: 80%;
    bottom: 5%;
  }
  .tags-icon {
    color: ${(props) => props.theme.css.lightVariantColor};
    align-self: center;
    margin-left: 0.5em;
  }
  position: relative;
  svg {
    align-self: center;
    margin-right: 0.5em;
  }
  .tag-list-header {
    border-top-left-radius: 0.25em;
    border-top-right-radius: 0.25em;
    padding: 0.5em;
    background-color: ${(props) => props.theme.css.primaryColor};
    h2 {
      color: ${(props) => props.theme.css.backgroundColor};
    }
    max-width: 30em;
    .close {
      margin-left: auto;
      cursor: pointer;
    }
  }
  .tag-list {
    padding-right: 0.5em;
    background-color: ${(props) => props.theme.css.backgroundColor};
    min-width: 30em;
    box-shadow: 0px 1px 2px rgba(0, 0, 0, 0.2);
  }
  padding: 0.5em;
  border-radius: 0.25em;
  margin: 0 0.5em 0 0.5em;
  .btn {
    max-height: 2.4em;
  }
`;
