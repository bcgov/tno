import styled from 'styled-components';

export const ContentTranscriptForm = styled.div`
  display: flex;
  flex-direction: column;

  .row-margins {
    margin-top: 2%;
  }

  .top-spacer {
    margin-top: 1.16em;
  }

  .content-body {
    display: flex;
    flex-flow: column;
    min-height: 100%;
    overflow: hidden;

    .quill {
      flex: 1;

      .ql-editor {
        height: 400px;
      }
    }
  }
`;
