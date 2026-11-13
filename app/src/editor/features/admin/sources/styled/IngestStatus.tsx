import styled from 'styled-components';

import { Col } from '@/shared/components/flex';

export const IngestStatus = styled(Col)`
  input[name$='startAt'],
  input[name$='stopAt'] {
    width: 115px;
  }
`;
