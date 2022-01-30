import styled from 'styled-components';

import { ButtonVariant, IButtonProps } from '.';

export const Button = styled.button<IButtonProps>`
  margin: 1px 2px 1px 2px;
  text-decoration: ${(props) => (props.variant === ButtonVariant.link ? 'underline' : 'none')};
  display: inline-block;
  font-weight: 400;
  text-align: center;
  vertical-align: middle;
  user-select: none;
  border: 1px solid transparent;
  padding: 0.375rem 0.75rem;
  font-size: 1rem;
  line-height: 1.6;
  border-radius: 0.25rem;
  transition: color 0.15s ease-in-out, background-color 0.15s ease-in-out,
    border-color 0.15s ease-in-out, box-shadow 0.15s ease-in-out;
  overflow: visible;
  text-transform: none;
  cursor: pointer;

  &[disabled] {
    background-color: grey;
    cursor: not-allowed;

    &:hover {
      filter: brightness(75%);
    }
  }

  color: ${(props) => {
    switch (props.variant) {
      case ButtonVariant.warning:
      case ButtonVariant.link:
        return '#1a5a96';
      default:
        return '#ffffff';
    }
  }};
  background-color: ${(props) => {
    switch (props.variant) {
      case ButtonVariant.primary:
        return '#38598a';
      case ButtonVariant.secondary:
        return '#6c757d';
      case ButtonVariant.success:
        return '#43893e';
      case ButtonVariant.info:
        return '#96c0e6';
      case ButtonVariant.warning:
        return '#f9ca54';
      case ButtonVariant.danger:
        return '#d93e45';
      case ButtonVariant.link:
        return 'transparent';
      default:
        return '#38598a';
    }
  }};
  border-color: ${(props) => {
    switch (props.variant) {
      case ButtonVariant.primary:
        return '#38598a';
      case ButtonVariant.secondary:
        return '#6c757d';
      case ButtonVariant.success:
        return '#43893e';
      case ButtonVariant.info:
        return '#96c0e6';
      case ButtonVariant.warning:
        return '#f9ca54';
      case ButtonVariant.danger:
        return '#d93e45';
      case ButtonVariant.link:
        return 'transparent';
      default:
        return '#38598a';
    }
  }};

  &:hover {
    color: ${(props) => {
      switch (props.variant) {
        case ButtonVariant.warning:
        case ButtonVariant.danger:
          return '#212529';
        case ButtonVariant.link:
          return '#0631f3';
        default:
          return '#fff';
      }
    }};
    background-color: ${(props) => {
      switch (props.variant) {
        case ButtonVariant.primary:
          return '#2d476f';
        case ButtonVariant.secondary:
          return '#5a6268';
        case ButtonVariant.success:
          return '#366f32';
        case ButtonVariant.info:
          return '#77addf';
        case ButtonVariant.warning:
          return '#f8bf2f';
        case ButtonVariant.danger:
          return '#c9282f';
        case ButtonVariant.link:
          return 'transparent';
        default:
          return '#fff';
      }
    }};
    border-color: ${(props) => {
      switch (props.variant) {
        case ButtonVariant.primary:
          return '#294266';
        case ButtonVariant.secondary:
          return '#545b62';
        case ButtonVariant.success:
          return '#32662e';
        case ButtonVariant.info:
          return '#6da7dc';
        case ButtonVariant.warning:
          return '#f7bb23';
        case ButtonVariant.danger:
          return '#be262c';
        case ButtonVariant.link:
          return 'transparent';
        default:
          return '#fff';
      }
    }};
  }

  &:focus {
    outline: 0;
    box-shadow: ${(props) => {
      switch (props.variant) {
        case ButtonVariant.primary:
          return '0 0 0 0.2rem rgb(86 114 156 / 50%)';
        case ButtonVariant.secondary:
          return '0 0 0 0.2rem rgb(130 138 145 / 50%)';
        case ButtonVariant.success:
          return '0 0 0 0.2rem rgb(95 155 91 / 50%)';
        case ButtonVariant.info:
          return '0 0 0 0.2rem rgb(132 169 202 / 50%)';
        case ButtonVariant.warning:
          return '0 0 0 0.2rem rgb(217 177 78 / 50%)';
        case ButtonVariant.danger:
          return '0 0 0 0.2rem rgb(223 91 97 / 50%)';
        case ButtonVariant.link:
          return '0 0 0 0.2rem rgb(56 89 138 / 50%)';
        default:
          return 'none';
      }
    }};
    color: ${(props) => {
      switch (props.variant) {
        case ButtonVariant.link:
          return '#0631f3';
      }
    }};
  }
`;

export default Button;
