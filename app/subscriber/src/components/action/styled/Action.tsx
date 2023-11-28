import styled from 'styled-components';

import { IActionProps } from '../Action';

export const Action = styled.div<IActionProps>`
  display: flex;
  flex-direction: ${(props) => props.direction ?? 'row'};
  gap: 0.5rem;
  align-items: center;
  cursor: pointer;

  font-size: 1rem;
  line-height: unset;
  font-weight: 400;
  max-height: 26px;

  &:hover * {
    color: ${(props) => props.theme.css.linkPrimaryHoverColor};
    filter: ${(props) => props.theme.css.dropShadow};
  }

  &:hover svg * {
    color: ${(props) => props.theme.css.linkPrimaryHoverColor};
    filter: ${(props) => props.theme.css.dropShadow};
  }

  &:active * {
    color: ${(props) => props.theme.css.linkPrimaryActiveColor};
  }

  &:active svg * {
    color: ${(props) => props.theme.css.linkPrimaryActiveColor};
  }

  label {
    color: ${(props) => props.theme.css.linkPrimaryColor};
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
    color: ${(props) => props.theme.css.iconPrimaryColor};
  }
`;
