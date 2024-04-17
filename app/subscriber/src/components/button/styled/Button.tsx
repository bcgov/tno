import styled from 'styled-components';

import { IButtonProps } from '../Button';

export const Button = styled.button<IButtonProps>`
  display: flex;
  flex-direction: row;
  gap: 0.5rem;
  align-items: center;
  justify-content: center;
  padding: 0.25rem 0.75rem;
  border-radius: 0.5rem;
  outline: inherit;

  font-size: 1rem;
  line-height: unset;
  font-weight: 400;

  &:not([disabled]) {
    cursor: pointer;
  }

  background: ${(props) => {
    switch (props.variant) {
      case 'secondary':
      case 'info':
      case 'link':
        return !props.disabled ? props.theme.css.bkWhite : props.theme.css.btnGrayColor;
      case 'success':
        return !props.disabled ? props.theme.css.btnBkSuccess : props.theme.css.btnGrayColor;
      case 'primary':
      default:
        return !props.disabled ? props.theme.css.btnBkPrimary : props.theme.css.btnGrayColor;
    }
  }};
  color: ${(props) => {
    switch (props.variant) {
      case 'info':
      case 'link':
      case 'secondary':
        return props.theme.css.btnBkPrimary;
      case 'success':
        return props.theme.css.btnSuccessColor;
      case 'primary':
      default:
        return props.theme.css.btnPrimaryColor;
    }
  }};
  border: ${(props) => {
    switch (props.variant) {
      case 'secondary':
        return `solid 1px ${props.theme.css.btnBkPrimary}`;
      case 'success':
        return props.theme.css.btnBkSuccess;
      case 'info':
      case 'link':
      case 'primary':
      default:
        return 'none';
    }
  }};

  svg {
    flex-shrink: 0;
    height: 16px;
    max-height: 16px;
    min-height: 16px;
    width: 16px;
    max-width: 16px;
    min-width: 16px;
    color: ${(props) => {
      switch (props.variant) {
        case 'secondary':
        case 'info':
        case 'link':
          return props.theme.css.btnBkPrimary;
        case 'primary':
        default:
          return props.theme.css.btnPrimaryColor;
      }
    }};
  }

  &:hover:not([disabled]) {
    box-shadow: ${(props) => props.theme.css.boxShadow};
  }

  &:active:not([disabled]) {
    background: ${(props) => props.theme.css.linkPrimaryActiveColor};
    box-shadow: ${(props) => props.theme.css.boxShadow};
  }
`;
