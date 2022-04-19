import styled from 'styled-components';

export const Upload = styled.div`
  display: flex;
  flex-direction: row;
  margin-right: 3%;
  .file-name {
    text-align: center;
  }
  .upl {
    color: #003366;
    margin: 1px 2px 1px 2px;
    margin-right: 5px;
    width: 124px;
    font-weight: 400;
    text-align: center;
    vertical-align: middle;
    user-select: none;
    border: 2px solid;
    padding: 0.375rem 0.75rem;
    font-size: 1rem;
    font-weight: 700;
    line-height: 1.6;
    border-radius: 0.25rem;
    transition: color 0.15s ease-in-out, background-color 0.15s ease-in-out,
      border-color 0.15s ease-in-out, box-shadow 0.15s ease-in-out;
    overflow: visible;
    text-transform: none;
    cursor: pointer;
  }

  .msg {
    color: #2e8540;
    font-weight: 700;
  }

  .full-msg {
    margin-left: 2%;
    background-color: red;
  }

  width: fit-content;
  margin-right: 5%;

  label {
    cursor: pointer;
  }

  .upload-wrapper {
    margin-right: 3%;
    background-color: red;
  }

  .btn {
    margin-left: 20px;
  }
`;
