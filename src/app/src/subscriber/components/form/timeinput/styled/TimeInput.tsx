import { InputHTMLAttributes } from 'react';
import styled from 'styled-components';

export const TimeInput = styled.div<InputHTMLAttributes<HTMLInputElement>>`
  margin-right: 0.5em;
  border: none;
  box-shadow: none;
  outline: none;
  .arrow-up {
    position: absolute;
    width: 0;
    height: 0;
    z-index: 2000;
    border-left: 10px solid transparent;
    border-right: 10px solid transparent;
    border-bottom: 10px solid ${(props) => props.theme.css.inputGrey};
    margin-left: 6em;
    margin-top: -9px;
  }
  .hour-menu,
  .minute-menu,
  .second-menu {
    max-height: 20em;
    overflow-y: auto;
    overflow-x: hidden;
  }
  .select-col {
    margin-right: 0.5em;
  }
  .time-opt {
    &.selected {
      background-color: ${(props) => props.theme.css.btnBkPrimary};
      color: white;
    }
    width: 2em;
    display: flex;
    justify-content: center;
    &:hover {
      cursor: pointer;
      background-color: ${(props) => props.theme.css.btnBkPrimary};
      color: white;
    }
  }
  width: ${(props) => props.width};
  .time-menu {
    font-size: 1.15em;
    box-shadow: 0 0 3px;
    z-index: 1000;
    max-height: 22em;
    position: absolute;
    background-color: ${(props) => props.theme.css.inputGrey};
    margin-left: 5em;
    padding: 1em;
    border: 1px solid ${(props) => props.theme.css.inputGrey};
  }
  .input-area {
    border: 1px solid ${(props) => props.theme.css.linePrimaryColor};
    border-radius: 0.25rem;
    display: flex;
    align-items: center;
    width: 8em;
    svg {
      margin-left: 0.5em;
      margin-bottom: 0.25em;
      color: ${(props) => props.theme.css.iconPrimaryColor};
      &:hover {
        cursor: pointer;
      }
    }
  }

  .col-header {
    font-weight: 700;
    text-decoration: underline;
  }

  .minute-menu,
  .second-menu {
    .time-opt {
      margin-left: 0.5em;
    }
  }

  label {
    font-weight: 700;
  }

  .required:after {
    content: ' *';
    color: ${(props) => props.theme.css.dangerColor};
    font-weight: 700;
  }

  .masked-input {
    border: none;
    border-radius: 0.25em;
    outline: none;
    padding: 0.375rem 0.75rem;

    width: 4.25em;
    :required {
      border-color: ${(props) => props.theme.css.inputRequiredBorderColor};
    }
  }

  p[role='alert'] {
    font-size: 0.85em;
    color: ${(props) => props.theme.css.fRedColor};
  }

  input[role='alert'] {
    border-color: ${(props) => props.theme.css.fRedColor};
    filter: grayscale(100%) brightness(65%) sepia(25%) hue-rotate(-50deg) saturate(600%)
      contrast(0.8);
  }

  input[disabled] {
    color: hsl(0, 0%, 50%);
    background-color: hsl(0, 0%, 95%);
  }
`;
