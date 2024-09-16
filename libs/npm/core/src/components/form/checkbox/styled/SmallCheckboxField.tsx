import styled from 'styled-components';

import { CheckboxVariant, ICheckboxProps } from '..';

export const SmallCheckboxField = styled.input<ICheckboxProps>`
  box-sizing: border-box;
  cursor: pointer;
  margin: 1px 2px 1px 2px;
  text-decoration: ${(props) => (props.variant === CheckboxVariant.link ? 'underline' : 'none')};
  display: inline-block;
  font-weight: 400;
  text-align: left;
  vertical-align: middle;
  user-select: text;
  border: 1px solid transparent;
  padding: 0.375rem 0.75rem;
  font-size: 1rem;
  line-height: 1.6;
  height: 1rem;
  width: 1rem;
  border-radius: 0.25rem;
  transition: color 0.15s ease-in-out, background-color 0.15s ease-in-out,
    border-color 0.15s ease-in-out, box-shadow 0.15s ease-in-out;
  overflow: visible;
  text-transform: none;

  color: ${(props) => {
    switch (props.variant) {
      case CheckboxVariant.warning:
      case CheckboxVariant.danger:
        return '#212529';
      case CheckboxVariant.link:
        return '#1a5a96';
      default:
        return props.theme.css.primaryColor;
    }
  }};

  background-color: ${(props) => {
    switch (props.variant) {
      case CheckboxVariant.primary:
        return props.theme.css.inputBackgroundColor;
      case CheckboxVariant.secondary:
        return '#6c757d';
      case CheckboxVariant.success:
        return '#43893e';
      case CheckboxVariant.info:
        return '#96c0e6';
      case CheckboxVariant.warning:
        return '#f9ca54';
      case CheckboxVariant.danger:
        return '#d93e45';
      case CheckboxVariant.link:
        return 'transparent';
      default:
        return '#38598a';
    }
  }};
  border-color: ${(props) => {
    switch (props.variant) {
      case CheckboxVariant.primary:
        return '#38598a';
      case CheckboxVariant.secondary:
        return '#6c757d';
      case CheckboxVariant.success:
        return '#43893e';
      case CheckboxVariant.info:
        return '#96c0e6';
      case CheckboxVariant.warning:
        return '#f9ca54';
      case CheckboxVariant.danger:
        return '#d93e45';
      case CheckboxVariant.link:
        return 'transparent';
      default:
        return '#38598a';
    }
  }};

  &:hover {
    color: ${(props) => {
      switch (props.variant) {
        case CheckboxVariant.warning:
        case CheckboxVariant.danger:
          return '#212529';
        case CheckboxVariant.link:
          return '#0631f3';
        default:
          return props.theme.css.primaryColor;
      }
    }};
    border-color: ${(props) => {
      switch (props.variant) {
        case CheckboxVariant.primary:
          return '#294266';
        case CheckboxVariant.secondary:
          return '#545b62';
        case CheckboxVariant.success:
          return '#32662e';
        case CheckboxVariant.info:
          return '#6da7dc';
        case CheckboxVariant.warning:
          return '#f7bb23';
        case CheckboxVariant.danger:
          return '#be262c';
        case CheckboxVariant.link:
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
        case CheckboxVariant.primary:
          return '0 0 0 0.2rem rgb(86 114 156 / 50%)';
        case CheckboxVariant.secondary:
          return '0 0 0 0.2rem rgb(130 138 145 / 50%)';
        case CheckboxVariant.success:
          return '0 0 0 0.2rem rgb(95 155 91 / 50%)';
        case CheckboxVariant.info:
          return '0 0 0 0.2rem rgb(132 169 202 / 50%)';
        case CheckboxVariant.warning:
          return '0 0 0 0.2rem rgb(217 177 78 / 50%)';
        case CheckboxVariant.danger:
          return '0 0 0 0.2rem rgb(223 91 97 / 50%)';
        case CheckboxVariant.link:
          return '0 0 0 0.2rem rgb(56 89 138 / 50%)';
        default:
          return 'none';
      }
    }};
    color: ${(props) => {
      switch (props.variant) {
        case CheckboxVariant.link:
          return '#0631f3';
      }
    }};
  }
`;
