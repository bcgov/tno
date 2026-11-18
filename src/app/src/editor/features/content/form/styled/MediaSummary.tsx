import styled from 'styled-components';

export const MediaSummary = styled.div`
  flex: 1 1 100%;
  display: flex;
  flex-direction: column;
  flex-wrap: wrap-reverse;

  hr {
    height: 0.05em;
  }

  .quill-summary {
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

  .media {
    flex: 0 0 0;
    justify-content: center;
    margin-left: auto;
    margin-right: auto;
    max-height: 350px;

    video {
      max-height: 300px;
    }
  }
`;
