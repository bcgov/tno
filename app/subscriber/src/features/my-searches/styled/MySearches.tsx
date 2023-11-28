import styled from 'styled-components';
import { Col } from 'tno-core';

export const MySearches = styled(Col)`
  /* option items in the tooltip menu */
  .option {
    cursor: pointer;
    &:hover {
      text-decoration: underline;
    }
  }
`;
