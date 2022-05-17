import styled from 'styled-components';

export const ServiceConfig = styled.div`
  display: flex;
  flex-direction: column;
  padding: 0 1em 0 1em;

  .actions {
    align-self: stretch;

    div[direction='row'] {
      width: unset;
    }
  }

  hr {
    width: 100%;
  }
`;
