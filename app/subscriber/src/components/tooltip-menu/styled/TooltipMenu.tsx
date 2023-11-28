import { Tooltip } from 'react-tooltip';
import styled from 'styled-components';

export const TooltipMenu = styled(Tooltip)`
  z-index: 999;
  border-radius: 0.25em;
  border: 1px solid ${({ theme }) => theme.linePrimaryColor};
  color: ${({ theme }) => theme.fPrimaryColor};
  .option&:hover {
    text-decoration: underline;
  }
`;
