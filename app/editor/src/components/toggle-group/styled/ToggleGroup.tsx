import styled from 'styled-components';
import { Row } from 'tno-core';

export const ToggleGroup = styled(Row)`
  padding: 0.25em;
  .active {
    background-color: ${(props) => props.theme.css.actionButtonColor};
    color: ${(props) => props.theme.css.backgroundColor};
  }
  .toggle-item {
    cursor: pointer;
    height: 1.75rem;
    border-radius: 0;
    border: 1px solid #a8aab3;
    box-shadow: 0px 1px 2px rgba(0, 0, 0, 0.2);
    font-size: 0.75em;
    padding-right: 0.5em;
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
`;
