import styled from 'styled-components';

export const Upload = styled.div`
  display: flex;
  flex-direction: row;
  width: fit-content;

  .indicator {
    align-self: center;
    padding-right: 0.25em;
  }

  .upload-image {
    height: 5rem;
    width: 5rem;
    align-self: center;
  }
  .drop-box {
    label {
      height: 100%;
      width: 100%;
    }
  }

  .text {
    align-self: center;
    button {
      margin-top: 0.5rem;
    }
  }
  .choose {
    font-weight: bold;
    font-size: 1rem;
  }
  .upload-box {
    border: 1px dotted #ccc;
    padding-bottom: 0.5rem;
  }
  .body {
    margin-top: 15%;
  }
  .file-action {
    display: grid;
    grid-template-columns: repeat(2, 1fr);
  }

  .file-name {
    border-color: ${(props) => props.theme.css.primaryColor};
    div {
      max-width: 13.5em;
      display: inline-block;
      overflow: hidden;
      text-overflow: ellipsis;
      white-space: nowrap;
    }
  }
  .delete {
    justify-self: flex-end;
  }

  .upload-image {
    color: ${(props) => props.theme.css.primaryLightColor};
    margin-bottom: 1rem;
  }

  audio {
    margin: auto;
    width: 100%;
  }
  video {
    height: 300px;
    width: 500px;
  }
`;
