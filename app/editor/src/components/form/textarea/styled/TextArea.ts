import { Col } from 'components/flex/col';
import styled from 'styled-components';

export const TextArea = styled(Col)`
  padding-right: 0.5em;

  p[role='alert'] {
    font-weight: 0.85em;
    color: ${(props) => props.theme.css.dangerColor};
  }

  textarea[role='alert'] {
    border-color: ${(props) => props.theme.css.dangerColor};
    filter: grayscale(100%) brightness(65%) sepia(25%) hue-rotate(-50deg) saturate(600%)
      contrast(0.8);
  }
`;
