import styled from 'styled-components';

export const ContentClipForm = styled.div`
  margin: 1em;

  min-width: 70em;
  background-color: white;
  padding: 0.5em 1em 1em 1em;

  .path {
    & > div b {
      cursor: pointer;
      text-decoration: underline;
    }
  }

  .folder {
    list-style: none;
    padding: 0;
    margin: 0.5em 0 1em 0;

    & > li {
      & > div {
      }

      & > div svg {
        margin-right: 1em;
      }

      div span {
        margin-right: 1em;
      }
    }
  }

  .video {
    width: 100%;
  }

  .video-buttons {
    margin-top: 20px;
  }

  .create-clip {
    margin-top: 35px;
  }

  .start-end {
    margin-left: 4px;
    font-weight: bold;
  }

  ul {
    display: flex;
    flex-direction: column;
  }

  .navigate {
    cursor: pointer;
    text-decoration: underline;
  }

  .file {
    cursor: pointer;
    text-decoration: underline;
  }

  .hidden {
    display: none;
  }

  .stream {
    cursor: pointer;
    color: darkblue;
  }

  .download {
    cursor: pointer;
    color: darkblue;
  }

  .delete {
    cursor: pointer;
    color: red;
  }

  .video {
    min-height: max-content;
  }

  video {
    height: calc(100% - 0.5em);
    width: auto;
  }

  .header {
    display: flex;
    font-weight: bold;
    margin-top: 20px;
    flex: 1 1 0;
  }

  .list-row {
    padding-top: 20px;
    padding-bottom: 20px;
  }

  .file-table {
    min-width: 1000px;
  }

  .lc-header {
    font-size: 14pt;
    font-weight: bold;
    padding-top: 20px;
    padding-bottom: 20px;
  }

  .target {
    padding-top: 7px;
    margin-left: 5px;
    margin-top: 1px;
    height: 40px;
  }

  .editing {
    border-top: 1px solid black;
    padding-top: 20px;
    margin-top: 30px;
    font-size: 14pt;
    width: 100%;
  }

  .navigate-up {
    margin-bottom: 20px;
  }
`;
