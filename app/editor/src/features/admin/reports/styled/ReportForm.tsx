import styled from 'styled-components';
import { FormPage } from 'tno-core';

export const ReportForm = styled(FormPage)`
  display: flex;
  flex-direction: column;
  align-items: center;

  .back-button {
    align-self: start;
    margin-bottom: 3%;
  }

  .code {
    .editor {
      padding: 0.375rem 0.75rem;
      box-sizing: border-box;
      display: inline-block;
      font-weight: 400;
      text-align: left;
      user-select: text;
      border: 1px solid rgb(96, 96, 96);
      font-size: 1rem;
      line-height: 1.6;
      border-radius: 0.25rem;
      overflow: auto;
      /* color: rgb(0, 51, 102); */
      transition: color 0.15s ease-in-out 0s, background-color 0.15s ease-in-out 0s,
        border-color 0.15s ease-in-out 0s, box-shadow 0.15s ease-in-out 0s;
      resize: both;
      max-width: calc(100vw - 140px);
    }
  }

  .form {
    width: 100%;
  }

  .preview-report {
    height: 100%;
    border: solid 2px ${(props) => props.theme.css.primaryColor};
    border-radius: 0.5rem;
    display: flex;
    flex-flow: column;
    box-shadow: 0 3px 15px rgb(0 0 0 / 0.5);
    margin-top: 1rem;

    .preview-subject {
      padding: 1rem;
      background-color: ${(props) => props.theme.css.primaryLightColor};
      color: #fff;
    }

    .preview-body {
      padding: 1rem;
      max-height: calc(100vh - 600px);
      overflow-y: auto;
    }
  }
`;
