import { InputHTMLAttributes } from 'react';
import styled from 'styled-components';

export const TimeInput = styled.div<InputHTMLAttributes<HTMLInputElement>>`
  & {
    margin-right: 0.5em;
    width: ${(props) => props.width};
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
    border-radius: 0.25rem;
    border: 1px solid #606060;
    line-height: 1.6;
    padding: 0.375rem 0.75rem;
    width: ${(props) => `calc(${props.width} - 1.5rem - 0.5em)`};

    :required {
      border-color: ${(props) => props.theme.css.inputRequiredBorderColor};
    }

    :focus {
      outline: 0;
      box-shadow: 0 0 0 0.2rem #2684ff;
    }
  }

  input[role='alert'] {
    border-color: ${(props) => props.theme.css.dangerColor};
    filter: grayscale(100%) brightness(65%) sepia(25%) hue-rotate(-50deg) saturate(600%)
      contrast(0.8);
  }

  input[disabled] {
    color: hsl(0, 0%, 50%);
    background-color: hsl(0, 0%, 95%);
  }
`;
