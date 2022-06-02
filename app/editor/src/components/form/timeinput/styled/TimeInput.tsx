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
    height: 2.125rem;
  }
`;
