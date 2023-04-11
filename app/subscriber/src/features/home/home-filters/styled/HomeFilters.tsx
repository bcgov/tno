import styled from 'styled-components';
import { Row } from 'tno-core';

export const HomeFilters = styled(Row)`
  /* special use case for buttons */
  @media (max-width: 500px) {
    margin-left: 0.5rem;
    button {
      min-width: 4rem !important;
    }
    font-size: 0.45em !important;
  }
  @media (min-width: 500px) {
    font-size: 0.5em !important;
  }
`;
