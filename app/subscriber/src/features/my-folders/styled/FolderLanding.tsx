import styled from 'styled-components';
import { Row } from 'tno-core';

export const FolderLanding = styled(Row)<{ split?: boolean }>`
  width: 100%;
  .left-side {
    width: ${({ split }) => (split ? '50%' : '100%')};
  }
  .right-side {
    width: ${({ split }) => (split ? '50%' : '0%')};
    transition: width 0.5s ease-in-out;
  }
`;
