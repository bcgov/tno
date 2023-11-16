import styled from 'styled-components';

export const ContentTranscriptForm = styled.div`
  flex: 1 1 100%;
  display: flex;
  flex-direction: column;
  margin: 1em;

  .quill-body {
    flex: 1;
    display: flex;
    flex-flow: column;
    padding-right: 0.5rem;
    min-width: 500px;

    .quill {
      flex: 1;

      .ql-editor {
        // TODO: Use Flexbox instead.
        height: 400px;
      }
    }
  }
`;
