import styled from 'styled-components';

import { IMenuButtonProps } from '../MenuButton';

export const MenuButton = styled.div<IMenuButtonProps>`
  background: ${(props) =>
    props.active
      ? props.theme.css.btnBkPrimary
      : props.disabled
      ? props.theme.css.btnGrayColor
      : 'unset'};
  color: ${(props) =>
    props.active
      ? props.theme.css.btnPrimaryColor
      : props.disabled
      ? props.theme.css.fSecondaryColor
      : props.theme.css.fPrimaryColor};
  cursor: ${(props) => (props.active ? 'unset' : props.disabled ? 'not-allowed' : 'pointer')};
  text-transform: uppercase;
  padding: 0.25rem 1rem;
  display: flex;
  flex-direction: row;
  align-items: center;
  justify-items: center;

  &:hover {
    background: ${(props) =>
      !props.active ? props.theme.css.btnBkSecondary : props.theme.css.btnBkPrimary};
  }
`;
