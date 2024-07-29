import styled from 'styled-components';
import { Row } from 'tno-core';

export const Tags = styled(Row)`
  align-items: flex-end;
  position: relative;
  .tags-icon {
    color: #9c9c9c;
    align-self: center;
    width: 2.5rem;
    height: 1.5rem;
  }
  position: relative;
  svg {
    align-self: center;
    margin-right: 0.5em;
  }
  border-radius: 0.25em;
  .button {
    max-height: 2.4em;
  }
`;
