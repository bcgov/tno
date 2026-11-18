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
    filter: ${(props) => props.theme.css.dropShadow};
    cursor: pointer;
  }

  &:active:not([disabled]) * {
    color: ${(props) => props.theme.css.linkPrimaryActiveColor};
    cursor: pointer;
  }

  &:active:not([disabled]) svg * {
    color: ${(props) => props.theme.css.linkPrimaryActiveColor};
    cursor: pointer;
  }

  label {
    color: ${(props) =>
      !props.disabled ? props.theme.css.linkPrimaryColor : props.theme.css.linkGrayColor};
    text-transform: uppercase;
    cursor: ${(props) => (!props.disabled ? 'pointer' : 'default')};
  }

  svg {
    flex-shrink: 0;
    height: ${(props) => props.size};
    max-height: ${(props) => props.size};
    min-height: ${(props) => props.size};
    width: ${(props) => props.size};
    max-width: ${(props) => props.size};
    min-width: ${(props) => props.size};
    color: ${(props) =>
      !props.disabled ? props.theme.css.iconPrimaryColor : props.theme.css.iconGrayColor};
  }
`;
