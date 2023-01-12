import styled from 'styled-components';

import { FieldSize, TextVariant } from '../..';
import { ITextAreaProps } from '..';

export const TextAreaField = styled.textarea<ITextAreaProps>`
  padding: 0.375rem 0.75rem;
  box-sizing: border-box;
  text-decoration: ${(props) => (props.variant === TextVariant.link ? 'underline' : 'none')};
  display: inline-block;
  width: ${(props) => props.width};
  min-width: ${FieldSize.Tiny};
  font-weight: 400;
  text-align: left;
  vertical-align: middle;
  user-select: text;
  border: 1px solid transparent;
  font-size: 1rem;
  line-height: 1.6;
  border-radius: 0.25rem;
  transition: color 0.15s ease-in-out, background-color 0.15s ease-in-out,
    border-color 0.15s ease-in-out, box-shadow 0.15s ease-in-out;
  overflow: visible;
  text-transform: none;

  color: ${(props) => {
    switch (props.variant) {
      case TextVariant.warning:
      case TextVariant.danger:
        return '#212529';
      case TextVariant.link:
        return '#1a5a96';
      default:
        return props.theme.css.primaryColor;
    }
  }};

  background-color: ${(props) => {
    switch (props.variant) {
      case TextVariant.primary:
        return props.theme.css.inputBackgroundColor;
      case TextVariant.secondary:
        return '#6c757d';
      case TextVariant.success:
        return '#43893e';
      case TextVariant.info:
        return '#96c0e6';
      case TextVariant.warning:
        return '#f9ca54';
      case TextVariant.danger:
        return '#d93e45';
      case TextVariant.link:
        return 'transparent';
      default:
        return '#38598a';
    }
  }};
  border-color: ${(props) => {
    switch (props.variant) {
      case TextVariant.primary:
        return props.required ? props.theme.css.inputRequiredBorderColor : '#606060';
      case TextVariant.secondary:
        return props.required ? props.theme.css.inputRequiredBorderColor : '#6c757d';
      case TextVariant.success:
        return '#43893e';
      case TextVariant.info:
        return '#96c0e6';
      case TextVariant.warning:
        return '#f9ca54';
      case TextVariant.danger:
        return '#d93e45';
      case TextVariant.link:
        return 'transparent';
      default:
        return '#38598a';
    }
  }};

  &:hover {
    color: ${(props) => {
      switch (props.variant) {
        case TextVariant.warning:
        case TextVariant.danger:
          return '#212529';
        case TextVariant.link:
          return '#0631f3';
        default:
          return props.theme.css.primaryColor;
      }
    }};
    border-color: ${(props) => {
      switch (props.variant) {
        case TextVariant.primary:
          return '#294266';
        case TextVariant.secondary:
          return '#545b62';
        case TextVariant.success:
          return '#32662e';
        case TextVariant.info:
          return '#6da7dc';
        case TextVariant.warning:
          return '#f7bb23';
        case TextVariant.danger:
          return '#be262c';
        case TextVariant.link:
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
        case TextVariant.primary:
          return '0 0 0 0.2rem #2684FF';
        case TextVariant.secondary:
          return '0 0 0 0.2rem rgb(130 138 145 / 50%)';
        case TextVariant.success:
          return '0 0 0 0.2rem rgb(95 155 91 / 50%)';
        case TextVariant.info:
          return '0 0 0 0.2rem rgb(132 169 202 / 50%)';
        case TextVariant.warning:
          return '0 0 0 0.2rem rgb(217 177 78 / 50%)';
        case TextVariant.danger:
          return '0 0 0 0.2rem rgb(223 91 97 / 50%)';
        case TextVariant.link:
          return '0 0 0 0.2rem rgb(56 89 138 / 50%)';
        default:
          return 'none';
      }
    }};
    color: ${(props) => {
      switch (props.variant) {
        case TextVariant.link:
          return '#0631f3';
      }
    }};
  }
`;
