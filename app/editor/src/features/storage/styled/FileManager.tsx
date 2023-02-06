import styled from 'styled-components';

export const FileManager = styled.div`
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

  .create-clip {
    margin-top: 1.45em;
  }

  label {
    font-weight: bold;
  }
`;
