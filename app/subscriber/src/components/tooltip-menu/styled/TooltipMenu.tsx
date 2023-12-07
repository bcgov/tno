import { Tooltip } from 'react-tooltip';
import styled from 'styled-components';

export const TooltipMenu = styled(Tooltip)`
  input {
    max-height: 2em;
  }
  z-index: 999;
  border-radius: 0.25em;
  border: 1px solid ${({ theme }) => theme.linePrimaryColor};
  background-color: ${({ theme }) => theme.bgPrimaryColor};
  color: ${({ theme }) => theme.fPrimaryColor};
  opacity: 1;
  .option&:hover {
    text-decoration: underline;
  }
`;
