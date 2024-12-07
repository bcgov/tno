import styled from 'styled-components';

export const ContentForm = styled.div`
  height: 100%;
  width: 100%;

  .toolbar-status {
    min-width: 150px;
  }

  .toolbar-alert {
    max-width: 200px;
  }

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

  .src-cpy {
    border: solid 1px #606060;
    border-left: none;
    border-top-right-radius: 0.25em;
    border-bottom-right-radius: 0.25em;
  }

  .disabled {
    color: ${(props) => props.theme.css.lightVariantColor};
  }

  .icon-button {
    &:not(.disabled) {
      color: ${(props) => props.theme.css.actionIconColor};
    }
    align-self: center;
    height: 1.5em;
    width: 1.5em;
    padding: 0.45em;

    &:not(.disabled):hover {
      cursor: pointer;
      color: ${(props) => props.theme.css.lightAccentColor};
    }
  }

  .source-url {
    border-right: none;
    border-top-right-radius: 0;
    border-bottom-right-radius: 0;
  }

  .headline {
    height: 2.5em;
  }

  .checkbox-column {
    margin-left: 2em;

    & > div:first-child {
      max-height: 12em;
    }
  }
  .checkbox-byline {
    padding-right: 0.5em;
  }

  .checkbox-cbra {
    margin-left: 2em;
    margin-top: 2em;
    padding-right: 0.5em;
  }

  .content-properties {
    flex-grow: 1;
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

  .tab .spinner {
    color: #494949;
  }

  .content-col {
    display: block;
  }

  .submit-buttons {
    flex: 1;
    justify-content: flex-end;
    align-items: center;
    padding: 0.5rem;

    .allow-no-file {
      min-width: 100px;
    }
  }

  .content-status {
    flex: 1 1 auto;
    margin: 0 1em 0 1em;
    padding: 0 1em 0 1em;
    align-content: center;
    justify-content: center;
    background-color: ${(props) => props.theme.css.tableColor};
    border-radius: 0.5em;

    & > span {
      color: ${(props) => props.theme.css.primaryColor};
    }
  }

  .approve-transcript {
    padding-left: 1em;
  }

  .allow-no-file {
    display: flex;
    align-items: center;
  }

  .type-Proactive {
    color: green;
  }
  .type-disabled {
    font-style: italic;
  }

  .multi-group {
    align-items: center;
    justify-content: center;
    background-color: #faf9f7;
    min-height: 4em;
    min-width: fit-content;
    padding: 0.5em;
    border-radius: 0.25em;
  }

  .tab-section {
    flex-flow: row;
    gap: 0.5rem;
  }

  .media {
    display: flex;
    flex-direction: column;
    justify-content: center;
  }

  .tab-layout {
    // width: 65%;
  }

  .other-series-select {
    width: 30ch;
  }

  .topic-select {
    width: 30ch;
  }
`;
