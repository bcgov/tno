import styled from 'styled-components';
import { Col } from 'tno-core/dist/components/flex/col';

export const ReachEarnedMedia = styled(Col)`
  & > div {
    .rem-label {
      display: flex;
      align-content: center;
      align-items: center;
      flex: 1;
      margin-right: 0.5em;
    }
  }
`;
