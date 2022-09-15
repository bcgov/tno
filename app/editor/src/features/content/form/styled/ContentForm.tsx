import styled from 'styled-components';

export const ContentForm = styled.div`
  height: 100%;

  .minimize-details {
    margin: 1em 0 0 0;
    padding: 0;
  }

  #txa-summary {
    height: 6.25em;
  }

  .condensed {
    border: solid 1px grey;
    border-radius: 0.25em;
    .popout {
      cursor: pointer;
    }
    .popout:hover {
      color: #3498db;
    }
  }

  .checkbox-column {
    margin-left: 2em;

    & > div:first-child {
      max-height: 12em;
    }
  }

  .content-properties {
    padding-top: 1em;
  }

  .licenses {
    padding-left: 2em;
  }

  .hidden {
    display: none;
  }

  .tab-container {
    height: calc(100vh - 580px);
    overflow-y: auto;
  }

  .tab-container div:has([name='transcription']) {
    height: 90%;

    textarea[name='transcription'] {
      flex: auto;
      min-height: 65px;
      resize: none;
    }
  }

  .tabs.small {
  }

  .tabs.large {
    .tab-container {
      height: 100%;
    }
  }
`;
