import styled from 'styled-components';

export const ContentForm = styled.div`
  height: 100%;
  z-index: 1;

  .minimize-details {
    margin: 1em 0 0 0;
    padding: 0;
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

  .tabs.fit {
    .tab-container {
      height: calc(100vh - 580px);
      overflow-y: auto;
    }
  }

  .tabs.small {
  }

  .tabs.large {
    .tab-container {
      height: 100%;
    }
  }

  .tabs.expand {
    .tab-container {
      margin-bottom: 3em;
    }
  }

  .tab .spinner {
    margin-left: 0.5em;
  }

  .submit-buttons {
    padding-top: 0.5em;
  }
`;
