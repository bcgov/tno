import { InputHTMLAttributes } from 'react';
import MaskedInput from 'react-text-mask';
import styled from 'styled-components';

export const TimeInput = styled(MaskedInput)<InputHTMLAttributes<HTMLInputElement>>`
  :required {
    border-color: ${(props) => props.theme.css.inputRequiredBorderColor};
  }

  :focus {
    outline: 0;
    box-shadow: 0 0 0 0.2rem #2684ff;
  }

  border-radius: 0.25rem;
  height: 2.5rem;
  width: ${(props) => props.width};
`;
