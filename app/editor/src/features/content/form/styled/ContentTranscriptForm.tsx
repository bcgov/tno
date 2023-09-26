import styled from 'styled-components';

export const ContentTranscriptForm = styled.div`
  margin: 1em;
  height: 90%;

  textarea[name='body'] {
    flex: auto;
    height: 90%;
  }

  .summary {
    flex: 1;
    padding-right: 0.5rem;
  }

  .quill {
    height: 295px;
  }
`;
