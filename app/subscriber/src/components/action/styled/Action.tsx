import styled from 'styled-components';

import { IActionProps } from '../Action';

export const Action = styled.div<IActionProps>`
  display: flex;
  flex-direction: ${(props) => props.direction ?? 'row'};
  gap: 0.5rem;
  align-items: center;

  &:not([disabled]) {
    cursor: pointer;
  }

  font-size: 1rem;
  line-height: unset;
  font-weight: 400;
  max-height: 26px;

  &:hover:not([disabled]) * {
    color: ${(props) => props.theme.css.linkPrimaryHoverColor};
    filter: ${(props) => props.theme.css.dropShadow};
  }

  &:hover:not([disabled]) svg * {
    color: ${(props) => props.theme.css.linkPrimaryHoverColor};
    filter: ${(props) => props.theme.css.dropShadow};
  }

  &:active:not([disabled]) * {
    color: ${(props) => props.theme.css.linkPrimaryActiveColor};
  }

  &:active:not([disabled]) svg * {
    color: ${(props) => props.theme.css.linkPrimaryActiveColor};
  }

  label {
    color: ${(props) =>
      !props.disabled ? props.theme.css.linkPrimaryColor : props.theme.css.linkGrayColor};
    text-transform: uppercase;
    cursor: pointer;
  }

  svg {
    flex-shrink: 0;
    height: 20px;
    max-height: 20px;
    min-height: 20px;
    width: 20px;
    max-width: 20px;
    min-width: 20px;
    color: ${(props) =>
      !props.disabled ? props.theme.css.iconPrimaryColor : props.theme.css.iconGrayColor};
  }
`;
