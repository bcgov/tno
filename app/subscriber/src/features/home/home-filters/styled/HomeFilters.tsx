import styled from 'styled-components';
import { Row } from 'tno-core';

export const HomeFilters = styled(Row)`
  backgrond-color: "red";
  /* special use case for buttons */
  @media (max-width: 500px) {
    margin-left: 0.5rem;
    button {
      min-width: 4rem !important;
    }
    font-size: 0.45em !important;
  }
  @media (min-width: 500px) {
    font-size: 0.5em !important;
  }
  font-family: Roboto', sans-serif;
      button {
      min-width: 5rem;
      display: flex;
      justify-content: center;
      &.active {
        background-color: ${(props) => props.theme.css.defaultRed} !important;
        color: white;
      }
      &.inactive {
        background-color: ${(props) => props.theme.css.lightInactiveButton} !important;
        color: #7a7978;
      }
    }
`;
