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
    :not(.disabled) {
      color: ${(props) => props.theme.css.actionIconColor};
    }
    align-self: center;
    height: 1.5em;
    width: 1.5em;
    padding: 0.45em;
    :not(.disabled): hover {
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

  .tabs.expand {
    .tab-container {
      margin-bottom: 3em;
    }
  }

  .tab .spinner {
    margin-left: 0.5em;
    color: #494949;
  }

  .submit-buttons {
    justify-content: flex-end;
    padding-top: 4.5em;
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
`;
