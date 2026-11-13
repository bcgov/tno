import styled from 'styled-components';

import { Row } from '../../flex';

export const ToggleGroup = styled(Row)<{ activeColor?: string }>`
  padding: 0.25em;
  // position: relative;
  .active {
    background-color: ${(props) => props.activeColor ?? props.theme.css.actionButtonColor};
    color: ${(props) => props.theme.css.backgroundColor};
  }
  .toggle-item {
    :disabled {
      cursor: not-allowed;
    }
    position: relative;
    .dd-menu {
      left: 0;
      .dd-item {
        padding: 0.25em 0.5em;
        text-align: left;
        font-size: 1rem;
        width: 15em;
        color: black;
        &:hover {
          background-color: ${(props) => props.activeColor ?? props.theme.css.actionButtonColor};
          color: ${(props) => props.theme.css.backgroundColor};
        }
      }
      padding: 0.5em;
      border: 1px solid #a8aab3;
      border-radius: 0.25em;

      z-index: 100;
      background-color: ${(props) => props.theme.css.backgroundColor};
      min-width: 15em;
      position: absolute;
      margin-top: 2em;
    }
    cursor: pointer;
    height: 1.75rem;
    border-radius: 0;
    border: 1px solid #a8aab3;
    box-shadow: 0px 1px 2px rgba(0, 0, 0, 0.2);
    font-size: 0.75em;
  }

  .toggle-item:not(.active) {
    background-color: ${(props) => props.theme.css.backgroundColor};
  }
  .toggle-item:first-child {
    border-top-left-radius: 4px;
    border-bottom-left-radius: 4px;
  }
  .toggle-item:last-child {
    border-top-right-radius: 4px;
    border-bottom-right-radius: 4px;
  }
  .toggle-item:hover {
    color: ${(props) => props.theme.css.backgroundColor};
    background-color: ${(props) => props.activeColor ?? props.theme.css.actionButtonColor};
  }
  label {
    margin-right: 1em;
  }
`;
