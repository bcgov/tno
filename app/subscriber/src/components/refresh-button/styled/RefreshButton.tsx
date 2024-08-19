import { Action } from 'components/action';
import styled from 'styled-components';

import { IRefreshButtonProps } from '../RefreshButton';

export const RefreshButton = styled(Action)<IRefreshButtonProps>`
  svg {
    color: #04814d;
    &:hover {
      transform: rotate(-90deg);
    }
    &:active:not([disabled]) * {
      color: #26e194;
    }
  }
`;
