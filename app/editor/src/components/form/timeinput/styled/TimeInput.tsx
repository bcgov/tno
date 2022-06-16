import styled from 'styled-components';
import { Col } from 'tno-core';

export const TimeInput = styled(Col)`
  label {
    font-weight: 700;
  }

  margin-right: 0.5em;

  .required:after {
    content: ' *';
    color: ${(props) => props.theme.css.dangerColor};
    font-weight: 700;
  }

  .masked-input {
    :required {
      border-color: ${(props) => props.theme.css.inputRequiredBorderColor};
    }

    :focus {
      outline: 0;
      box-shadow: 0 0 0 0.2rem #2684ff;
    }

    border-radius: 0.25rem;
    height: 2.225rem;
    border: 1px solid #606060;
    line-height: 1.6;
  }

  p[role='alert'] {
    font-size: 0.85em;
    color: ${(props) => props.theme.css.dangerColor};
  }

  input[role='alert'] {
    border-color: ${(props) => props.theme.css.dangerColor};
    filter: grayscale(100%) brightness(65%) sepia(25%) hue-rotate(-50deg) saturate(600%)
      contrast(0.8);
  }
`;
