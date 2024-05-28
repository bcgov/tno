import styled from 'styled-components';
import { Row } from 'tno-core';

export const FolderLanding = styled(Row)<{ split?: boolean }>`
  width: 100%;
  .left-side {
    @media (max-width: 1100px) {
      width: 100%;
    }
    @media (min-width: 1100px) {
      width: ${({ split }) => (split ? '30%' : '100%')};
    }
    transition: width 0.5s ease-in-out;
  }
  .right-side {
    @media (max-width: 1100px) {
      width: 100%;
    }
    @media (min-width: 1100px) {
      width: ${({ split }) => (split ? '70%' : '100%')};
    }
    transition: width 0.5s ease-in-out;
  }
`;
