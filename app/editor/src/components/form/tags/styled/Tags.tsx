import styled from 'styled-components';
import { Row } from 'tno-core';

export const Tags = styled(Row)`
  padding: 0.5em;
  border-radius: 0.25em;
  margin: 0 0.5em 0 0.5em;
  .btn {
    align-self: flex-start;
    margin-top: 1.7em;
  }
  .clear-tags {
    align-items: center;
  }
`;
