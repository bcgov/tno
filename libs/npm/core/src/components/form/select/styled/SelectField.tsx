import Select from 'react-select';
import styled from 'styled-components';

import { ISelectProps, SelectVariant } from '..';

export const SelectField = styled(Select)<ISelectProps<any>>`
  margin-right: 0.5em;
  flex-grow: 1;

  &.alert {
    .rs__control {
      border-color: ${(props) => props.theme.css.dangerColor};
      filter: grayscale(100%) brightness(65%) sepia(25%) hue-rotate(-50deg) saturate(600%)
        contrast(0.8);
    }
  }

  .rs__control {
    text-decoration: ${(props) => (props.variant === SelectVariant.link ? 'underline' : 'none')};
    display: flex;
    width: ${(props) => props.width};
    font-weight: 400;
    text-align: left;
    vertical-align: middle;
    user-select: text;
    border-width: 1px;
    border-style: solid;
    font-size: 1rem;
    line-height: 1.6;
    border-radius: 0.25rem;
    transition: color 0.15s ease-in-out, background-color 0.15s ease-in-out,
      border-color 0.15s ease-in-out, box-shadow 0.15s ease-in-out;
    overflow: visible;
    text-transform: none;
    color: ${(props) => {
      switch (props.variant) {
        case SelectVariant.warning:
        case SelectVariant.danger:
          return '#212529';
        case SelectVariant.link:
          return '#1a5a96';
        default:
          return props.theme.css.primaryColor;
      }
    }};

    background-color: ${(props) => {
      switch (props.variant) {
        case SelectVariant.primary:
          return props.theme.css.inputBackgroundColor;
        case SelectVariant.secondary:
          return '#6c757d';
        case SelectVariant.success:
          return '#43893e';
        case SelectVariant.info:
          return '#96c0e6';
        case SelectVariant.warning:
          return '#f9ca54';
        case SelectVariant.danger:
          return '#d93e45';
        case SelectVariant.link:
          return 'transparent';
        default:
          return '#38598a';
      }
    }};

    border-color: ${(props) => {
      switch (props.variant) {
        case SelectVariant.primary:
          return props.required ? props.theme.css.inputRequiredBorderColor : '#606060';
        case SelectVariant.secondary:
          return props.required ? props.theme.css.inputRequiredBorderColor : '#6c757d';
        case SelectVariant.success:
          return '#43893e';
        case SelectVariant.info:
          return '#96c0e6';
        case SelectVariant.warning:
          return '#f9ca54';
        case SelectVariant.danger:
          return '#d93e45';
        case SelectVariant.link:
          return 'transparent';
        default:
          return '#38598a';
      }
    }};

    &:hover {
      color: ${(props) => {
        switch (props.variant) {
          case SelectVariant.warning:
          case SelectVariant.danger:
            return '#212529';
          case SelectVariant.link:
            return '#0631f3';
          default:
            return props.theme.css.primaryColor;
        }
      }};
    }

    &:focus {
      outline: 0;
      box-shadow: ${(props) => {
        switch (props.variant) {
          case SelectVariant.primary:
            return '0 0 0 0.2rem #2684FF';
          case SelectVariant.secondary:
            return '0 0 0 0.2rem rgb(130 138 145 / 50%)';
          case SelectVariant.success:
            return '0 0 0 0.2rem rgb(95 155 91 / 50%)';
          case SelectVariant.info:
            return '0 0 0 0.2rem rgb(132 169 202 / 50%)';
          case SelectVariant.warning:
            return '0 0 0 0.2rem rgb(217 177 78 / 50%)';
          case SelectVariant.danger:
            return '0 0 0 0.2rem rgb(223 91 97 / 50%)';
          case SelectVariant.link:
            return '0 0 0 0.2rem rgb(56 89 138 / 50%)';
          default:
            return 'none';
        }
      }};
    }
  }

  .rs__menu {
    width: ${(props) => props.width};
  }

  .clear {
    color: ${(props) => props.theme.css.primaryColor};
  }
`;
