import styled from 'styled-components';
import { Row } from 'tno-core';

export const HomeFilters = styled(Row)`
  margin-left: auto;
  font-size: 0.3em;
  align-items: center;
  button {
    min-width: fit-content;
    font-weight: bold;
    border: none;
    display: flex;
    justify-content: center;
    &.active {
      background-color: ${(props) => props.theme.css.btnRedColor};
      color: white;
    }
    &.inactive {
      background-color: ${(props) => props.theme.css.btnGrayColor};
      color: white;
    }
  }
  button:not(:last-child) {
    margin-right: 0.5em;
  }
`;
