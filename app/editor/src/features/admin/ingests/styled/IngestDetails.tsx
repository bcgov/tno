import styled from 'styled-components';

export const IngestDetails = styled.div`
  display: flex;
  flex-direction: row;
  gap: 1em;

  & > div {
    flex-grow: 1;
    flex-basis: max-content;
  }

  p {
    max-width: 30ch;
  }
`;
