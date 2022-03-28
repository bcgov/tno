import { Col } from 'components/flex/col';
import styled from 'styled-components';

export const Select = styled(Col)`
  p[role='alert'] {
    font-size: 0.85em;
    color: ${(props) => props.theme.css.dangerColor};
  }

  & > input {
    width: 0px !important; // Required to stop the input from using 100% width and causing horizontal overflow issues.
  }
`;
