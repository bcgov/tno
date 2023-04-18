import styled from 'styled-components';
import { Row } from 'tno-core';

export const DateFilter = styled(Row)`
  .react-datepicker__input-container,
  .react-datepicker-wrapper {
    padding-right: 0.2em;
    input {
      text-align: center;
      &:focus {
        outline: none;
      }
      max-width: 6.5em;
      font-weight: bold;
      background-color: transparent;
      border: none;
    }
    max-width: 6.5em;
  }
`;
