import styled from 'styled-components';

export const ContentSummaryForm = styled.div`
  height: 90%;

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
    height: 10em;
    padding: 0 0.5em 0 0.5em;
  }

  .multi-section {
    align-items: center;
    justify-content: space-evenly;
  }

  .toning {
    height: 0.5em;
    margin-top: 1.5em;
    padding: 0 0.5em 0 0.5em;
  }

  .multi-group {
    align-items: center;
    justify-content: center;
    background-color: #f2f2f2;
    min-height: 4em;
    min-width: fit-content;
    padding: 0.5em;
    border-radius: 0.25em;
    margin: 0 0.5em 0 0.5em;
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
`;
