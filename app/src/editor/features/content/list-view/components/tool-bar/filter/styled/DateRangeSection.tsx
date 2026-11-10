import styled from 'styled-components';
import { Row } from 'tno-core';

export const DateRangeSection = styled(Row)`
  margin-bottom: 0.25em;
  .clear {
    &:hover {
      cursor: pointer;
      color: ${(props) => props.theme.css.dangerColor};
    }
    color: ${(props) => props.theme.css.lightVariantColor};
  }
  .to-text {
    align-self: center;
    margin-right: 0.25em;
  }
  .frm-in {
    margin-bottom: unset;
  }
`;
