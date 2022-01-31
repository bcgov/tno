import styled from 'styled-components';

import { DropdownVariant, IDropdownProps } from '.';

export const Dropdown = styled.select<IDropdownProps>`
  margin: 1px 2px 1px 2px;
  text-decoration: ${(props) => (props.variant === DropdownVariant.link ? 'underline' : 'none')};
  display: inline-block;
  font-weight: 400;
  text-align: left;
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
  color: ${(props) => {
    switch (props.variant) {
      case DropdownVariant.warning:
      case DropdownVariant.danger:
        return '#212529';
      case DropdownVariant.link:
        return '#1a5a96';
      default:
        return props.theme.css.primaryColor;
    }
  }};
  background-color: ${(props) => {
    switch (props.variant) {
      case DropdownVariant.primary:
        return props.theme.css.inputBackgroundColor;
      case DropdownVariant.secondary:
        return '#6c757d';
      case DropdownVariant.success:
        return '#43893e';
      case DropdownVariant.info:
        return '#96c0e6';
      case DropdownVariant.warning:
        return '#f9ca54';
      case DropdownVariant.danger:
        return '#d93e45';
      case DropdownVariant.link:
        return 'transparent';
      default:
        return '#38598a';
    }
  }};
  border-color: ${(props) => {
    switch (props.variant) {
      case DropdownVariant.primary:
        return '#38598a';
      case DropdownVariant.secondary:
        return '#6c757d';
      case DropdownVariant.success:
        return '#43893e';
      case DropdownVariant.info:
        return '#96c0e6';
      case DropdownVariant.warning:
        return '#f9ca54';
      case DropdownVariant.danger:
        return '#d93e45';
      case DropdownVariant.link:
        return 'transparent';
      default:
        return '#38598a';
    }
  }};

  &:hover {
    color: ${(props) => {
      switch (props.variant) {
        case DropdownVariant.warning:
        case DropdownVariant.danger:
          return '#212529';
        case DropdownVariant.link:
          return '#0631f3';
        default:
          return props.theme.css.primaryColor;
      }
    }};
    border-color: ${(props) => {
      switch (props.variant) {
        case DropdownVariant.primary:
          return '#294266';
        case DropdownVariant.secondary:
          return '#545b62';
        case DropdownVariant.success:
          return '#32662e';
        case DropdownVariant.info:
          return '#6da7dc';
        case DropdownVariant.warning:
          return '#f7bb23';
        case DropdownVariant.danger:
          return '#be262c';
        case DropdownVariant.link:
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
        case DropdownVariant.primary:
          return '0 0 0 0.2rem rgb(86 114 156 / 50%)';
        case DropdownVariant.secondary:
          return '0 0 0 0.2rem rgb(130 138 145 / 50%)';
        case DropdownVariant.success:
          return '0 0 0 0.2rem rgb(95 155 91 / 50%)';
        case DropdownVariant.info:
          return '0 0 0 0.2rem rgb(132 169 202 / 50%)';
        case DropdownVariant.warning:
          return '0 0 0 0.2rem rgb(217 177 78 / 50%)';
        case DropdownVariant.danger:
          return '0 0 0 0.2rem rgb(223 91 97 / 50%)';
        case DropdownVariant.link:
          return '0 0 0 0.2rem rgb(56 89 138 / 50%)';
        default:
          return 'none';
      }
    }};
    color: ${(props) => {
      switch (props.variant) {
        case DropdownVariant.link:
          return '#0631f3';
      }
    }};
  }
`;

export default Dropdown;
