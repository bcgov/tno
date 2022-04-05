import styled from 'styled-components';

import { ISelectDateProps, SelectDateVariant } from '..';

export const SelectDate = styled.div<ISelectDateProps>`
  padding-right: 0.5em;

  .required:after {
    content: ' *';
    color: ${(props) => props.theme.css.dangerColor};
    font-weight: 700;
  }

  .react-datepicker-wrapper {
    min-width: 12ch;

    input {
      padding: 0.375rem 0.75rem;
      text-decoration: ${(props) =>
        props.variant === SelectDateVariant.link ? 'underline' : 'none'};
      display: flex;
      width: ${(props) => props.width};
      flex-direction: column;
      font-weight: 400;
      text-align: left;
      vertical-align: middle;
      user-select: text;
      border: 1px solid transparent;
      border-radius: 0.25em;
      font-size: 1rem;
      line-height: 1.6;
      transition: color 0.15s ease-in-out, background-color 0.15s ease-in-out,
        border-color 0.15s ease-in-out, box-shadow 0.15s ease-in-out;
      overflow: visible;
      text-transform: none;
      color: ${(props) => {
        switch (props.variant) {
          case SelectDateVariant.warning:
          case SelectDateVariant.danger:
            return '#212529';
          case SelectDateVariant.link:
            return '#1a5a96';
          default:
            return '#000000';
        }
      }};

      background-color: ${(props) => {
        switch (props.variant) {
          case SelectDateVariant.primary:
            return props.theme.css.inputBackgroundColor;
          case SelectDateVariant.secondary:
            return '#6c757d';
          case SelectDateVariant.success:
            return '#43893e';
          case SelectDateVariant.info:
            return '#96c0e6';
          case SelectDateVariant.warning:
            return '#f9ca54';
          case SelectDateVariant.danger:
            return '#d93e45';
          case SelectDateVariant.link:
            return 'transparent';
          default:
            return '#38598a';
        }
      }};
      border-color: ${(props) => {
        switch (props.variant) {
          case SelectDateVariant.primary:
            return props.required ? props.theme.css.inputRequiredBorderColor : '#606060';
          case SelectDateVariant.secondary:
            return props.required ? props.theme.css.inputRequiredBorderColor : '#6c757d';
          case SelectDateVariant.success:
            return '#43893e';
          case SelectDateVariant.info:
            return '#96c0e6';
          case SelectDateVariant.warning:
            return '#f9ca54';
          case SelectDateVariant.danger:
            return '#d93e45';
          case SelectDateVariant.link:
            return 'transparent';
          case SelectDateVariant.disabled:
            return '#606060';
          default:
            return '#38598a';
        }
      }};

      &:hover {
        color: ${(props) => {
          switch (props.variant) {
            case SelectDateVariant.warning:
            case SelectDateVariant.danger:
              return '#212529';
            case SelectDateVariant.link:
              return '#0631f3';
            default:
              return props.theme.css.primaryColor;
          }
        }};
        border-color: ${(props) => {
          switch (props.variant) {
            case SelectDateVariant.primary:
              return '#294266';
            case SelectDateVariant.secondary:
              return '#545b62';
            case SelectDateVariant.success:
              return '#32662e';
            case SelectDateVariant.info:
              return '#6da7dc';
            case SelectDateVariant.warning:
              return '#f7bb23';
            case SelectDateVariant.danger:
              return '#be262c';
            case SelectDateVariant.link:
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
            case SelectDateVariant.primary:
              return '0 0 0 0.2rem #2684FF';
            case SelectDateVariant.secondary:
              return '0 0 0 0.2rem rgb(130 138 145 / 50%)';
            case SelectDateVariant.success:
              return '0 0 0 0.2rem rgb(95 155 91 / 50%)';
            case SelectDateVariant.info:
              return '0 0 0 0.2rem rgb(132 169 202 / 50%)';
            case SelectDateVariant.warning:
              return '0 0 0 0.2rem rgb(217 177 78 / 50%)';
            case SelectDateVariant.danger:
              return '0 0 0 0.2rem rgb(223 91 97 / 50%)';
            case SelectDateVariant.link:
              return '0 0 0 0.2rem rgb(56 89 138 / 50%)';
            default:
              return 'none';
          }
        }};
        color: ${(props) => {
          switch (props.variant) {
            case SelectDateVariant.link:
              return '#0631f3';
          }
        }};
      }
    }
  }
`;
