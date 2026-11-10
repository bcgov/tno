import { Tooltip } from 'react-tooltip';
import styled from 'styled-components';

export const TooltipMenu = styled(Tooltip)`
  input {
    max-height: 2em;
  }
  z-index: 999;
  border-radius: 0.25em;
  /* Border, background and text colours were meant to come from theme.css.linePrimaryColor,
     bgPrimaryColor and fPrimaryColor, but read keys that do not exist and never applied. */
  opacity: 1;
  .option&:hover {
    text-decoration: underline;
  }
`;
