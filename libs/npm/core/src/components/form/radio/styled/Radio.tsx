import styled from 'styled-components';

import { IRadioProps, RadioVariant } from '..';

export const Radio = styled.input<IRadioProps>`
  label {
    cursor: hover;
    background-color: ${(props) => !!props.error && 'red'}
  }
  box-sizing: border-box;
  margin: 1px 2px 1px 2px;
  text-decoration: ${(props) => (props.variant === RadioVariant.link ? 'underline' : 'none')};
  font-weight: 400;
  text-align: left;
  height: 20px;
  width: 20px;
  vertical-align: middle;
  user-select: text;
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

  color: ${(props) => {
    switch (props.variant) {
      case RadioVariant.warning:
      case RadioVariant.danger:
        return '#212529';
      case RadioVariant.link:
        return '#1a5a96';
      default:
        return props.theme.css.primaryColor;
    }
  }};

  }};
  border-color: ${(props) => {
    switch (props.variant) {
      case RadioVariant.primary:
        return '#38598a';
      case RadioVariant.secondary:
        return '#6c757d';
      case RadioVariant.success:
        return '#43893e';
      case RadioVariant.info:
        return '#96c0e6';
      case RadioVariant.warning:
        return '#f9ca54';
      case RadioVariant.danger:
        return '#d93e45';
      case RadioVariant.link:
        return 'transparent';
      default:
        return '#38598a';
    }
  }};

  &:hover {
    color: ${(props) => {
      switch (props.variant) {
        case RadioVariant.warning:
        case RadioVariant.danger:
          return '#212529';
        case RadioVariant.link:
          return '#0631f3';
        default:
          return props.theme.css.primaryColor;
      }
    }};
    border-color: ${(props) => {
      switch (props.variant) {
        case RadioVariant.primary:
          return '#294266';
        case RadioVariant.secondary:
          return '#545b62';
        case RadioVariant.success:
          return '#32662e';
        case RadioVariant.info:
          return '#6da7dc';
        case RadioVariant.warning:
          return '#f7bb23';
        case RadioVariant.danger:
          return '#be262c';
        case RadioVariant.link:
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
        case RadioVariant.primary:
          return '0 0 0 0.2rem rgb(86 114 156 / 50%)';
        case RadioVariant.secondary:
          return '0 0 0 0.2rem rgb(130 138 145 / 50%)';
        case RadioVariant.success:
          return '0 0 0 0.2rem rgb(95 155 91 / 50%)';
        case RadioVariant.info:
          return '0 0 0 0.2rem rgb(132 169 202 / 50%)';
        case RadioVariant.warning:
          return '0 0 0 0.2rem rgb(217 177 78 / 50%)';
        case RadioVariant.danger:
          return '0 0 0 0.2rem rgb(223 91 97 / 50%)';
        case RadioVariant.link:
          return '0 0 0 0.2rem rgb(56 89 138 / 50%)';
        default:
          return 'none';
      }
    }};
    color: ${(props) => {
      switch (props.variant) {
        case RadioVariant.link:
          return '#0631f3';
      }
    }};
  }
`;
