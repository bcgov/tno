import styled from 'styled-components';

import { Col } from '../../../flex';

export const TextArea = styled(Col)`
  padding-right: 0.5em;

  label {
    svg {
      margin-left: 0.25em;
    }
  }

  .required:after {
    content: ' *';
    color: ${(props) => props.theme.css.dangerColor};
    font-weight: 700;
  }

  textarea[role='alert'] {
    border-color: ${(props) => props.theme.css.dangerColor};
    filter: grayscale(100%) brightness(65%) sepia(25%) hue-rotate(-50deg) saturate(600%)
      contrast(0.8);
  }
`;
