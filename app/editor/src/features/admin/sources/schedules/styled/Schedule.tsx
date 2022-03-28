import { Col } from 'components/flex/col';
import styled from 'styled-components';

export const Schedule = styled(Col)`
  p {
    margin-right: 0.5em;
  }

  input[name$='startAt'],
  input[name$='stopAt'] {
    width: 115px;
  }
`;
