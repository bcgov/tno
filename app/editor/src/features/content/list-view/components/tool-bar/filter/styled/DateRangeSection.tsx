import styled from 'styled-components';
import { Row } from 'tno-core';

export const DateRangeSection = styled(Row)`
  .clear {
    :hover {
      cursor: pointer;
      color: ${(props) => props.theme.css.dangerColor};
    }
    align-self: center;
    font-size: 1.25em;
    padding-bottom: 0.25em;
    padding-left: 0.25em;
    color: ${(props) => props.theme.css.lightVariantColor};
  }
  .to-text {
    align-self: center;
    margin-right: 0.25em;
  }
`;
