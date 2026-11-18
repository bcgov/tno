import styled from 'styled-components';

export const Upload = styled.div`
  display: flex;
  flex-direction: column;
  margin-top: 2.5rem;
  padding-bottom: 0.5rem;
  img,
  video,
  audio {
    margin-left: auto;
    margin-right: auto;
  }

  .file-name {
    margin-left: auto;
    margin-right: auto;
  }

  .upload-buttons {
    margin-left: auto;
    margin-right: auto;
    margin-top: 0.5em;
  }

  > div:first-child {
    justify-content: center;
    flex: 1 1 100%;
  }

  .upload-image {
    height: 5rem;
    width: 5rem;
    align-self: center;
  }

  .drop-box {
    label {
      display: flex;
      flex-direction: column;
      align-content: center;
      justify-content: center;
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
    margin-top: 3%;
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

  .image {
    object-fit: contain;
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
