import styled from 'styled-components';
import { Row } from 'tno-core';

export const TimeLogSection = styled(Row)`
  .action-button {
    align-self: center;
    padding-right: 0.25em;
    padding-top: 0.5em;
    color: ${(props) => props.theme.css.actionButtonColor};
    :hover {
      color: ${(props) => props.theme.css.lightVariantColor};
    }
    height: 1.75em;
    width: 2em;
    cursor: pointer;
  }
  .disabled-section {
    label {
      color: ${(props) => props.theme.css.lightVariantColor};
    }
  }
`;
