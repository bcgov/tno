import styled from 'styled-components';
import { Row } from 'tno-core';

export const DateFilter = styled(Row)`
  font-size: 1.2em;
  svg {
    cursor: pointer;
    color: ${(props) => props.theme.css.btnBkPrimary};
  }
  .date {
    margin-left: 1.5em;
    margin-right: 1.5em;
    display: flex;
    .calendar {
      align-self: center;
      height: 1.25em;
      width: 1.25em;
    }
  }
  .caret {
    transform: scale(1.5);
  }
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
