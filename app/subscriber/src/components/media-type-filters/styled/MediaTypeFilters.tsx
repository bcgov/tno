import styled from 'styled-components';
import { Row } from 'tno-core';

export const HomeFilters = styled(Row)`
  @media (min-width: 768px) {
    margin-left: auto;
  }
  @media (max-width: 768px) {
    margin-right: auto;
    margin-top: 1em;
  }
  font-size: 0.3em;
  margin-top: auto;
  margin-bottom: auto;
  align-items: center;

  button {
    min-width: fit-content;
    font-weight: bold;
    border: none;
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
