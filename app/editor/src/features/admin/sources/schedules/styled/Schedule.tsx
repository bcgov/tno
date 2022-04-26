import styled from 'styled-components';
import { Col } from 'tno-core/dist/components/flex/col';

export const Schedule = styled(Col)`
  min-width: 30em;
  width: 100%;
  display: flex;
  flex-direction: column;

  p {
    margin-right: 0.5em;
  }

  .actions {
    align-self: stretch;

    div[direction='row'] {
      width: unset;
    }
  }
`;
