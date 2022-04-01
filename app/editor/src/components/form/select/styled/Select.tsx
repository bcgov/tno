import styled from 'styled-components';
import { Col } from 'tno-core/dist/components/flex';

export const Select = styled(Col)`
  .required:after {
    content: ' *';
    color: ${(props) => props.theme.css.dangerColor};
  }

  p[role='alert'] {
    font-size: 0.85em;
    color: ${(props) => props.theme.css.dangerColor};
  }

  & > input {
    width: 0px !important; // Required to stop the input from using 100% width and causing horizontal overflow issues.
  }
`;
