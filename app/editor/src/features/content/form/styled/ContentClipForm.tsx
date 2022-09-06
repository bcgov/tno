import styled from 'styled-components';

export const ContentClipForm = styled.div`
  margin: 1em;

  background-color: white;

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

  video {
    height: 270px;
    width: 480px;
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

  .lc-header {
    font-size: 14pt;
    font-weight: bold;
    padding-top: 20px;
    padding-bottom: 20px;
  }

  .prefix {
    padding-top: 7px;
    margin-left: 5px;
    margin-top: 1px;
    height: 40px;
  }

  .start-end {
    border: 0;
    background-color: white;
    padding: 0;
  }

  .editing {
    border-top: 1px solid black;
    padding-top: 20px;
    margin-top: 30px;
    font-size: 14pt;
    width: 100%;
    display: flex;
    flex-direction: col;
  }

  .navigate-up {
    margin-bottom: 20px;
  }

  .required:after {
    content: ' *';
    color: ${(props) => props.theme.css.dangerColor};
    font-weight: 700;
  }
`;
