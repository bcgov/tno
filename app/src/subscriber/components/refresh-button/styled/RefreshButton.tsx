import styled from 'styled-components';

import { Action } from '@/subscriber/components/action';

import { IRefreshButtonProps } from '../RefreshButton';

export const RefreshButton = styled(Action)<IRefreshButtonProps>`
  &:not([disabled]) svg {
    color: #04814d;
    &:hover {
      transform: rotate(-90deg);
    }
    &:active * {
      color: #26e194;
    }
  }
`;
