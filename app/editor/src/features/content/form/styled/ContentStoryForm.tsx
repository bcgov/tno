import styled from 'styled-components';

export const ContentStoryForm = styled.div`
  display: flex;
  flex-direction: column;

  .row-margins {
    margin-top: 2%;
  }

  .top-spacer {
    margin-top: 1.16em;
  }

  .show-player {
    margin-left: 10px;
  }

  .add-time {
    margin-right: 0.5em;
  }

  .nlp-button {
    align-self: center;
    padding-right: 0.25em;
    height: 1.25em;
    width: 1.25em;
  }

  .transcription-section {
    align-items: center;
    padding-right: 1em;
  }

  .vl {
    border-left: 1px solid;
    padding: 0 0.5em 0 0.5em;
  }

  .video {
    align-self: center;
    max-width: fit-content;
  }

  .toning {
    height: 0.5em;
    margin-top: 1.5em;
    padding: 0 0.5em 0 0.5em;
  }

  .textarea {
    height: 50%;

    div {
      height: 100%;
    }
  }

  textarea[name='summary'] {
    height: 80%;
  }

  textarea[name='body'] {
    height: 80%;
  }

  .object-fit {
    object-fit: contain;
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
