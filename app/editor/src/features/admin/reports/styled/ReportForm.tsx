import styled from 'styled-components';
import { FormPage } from 'tno-core';

export const ReportForm = styled(FormPage)`
  display: flex;
  flex-direction: column;
  .back-button {
    align-self: start;
    margin-bottom: 3%;
  }
  align-items: center;
  .form-inputs {
    margin-top: 3%;
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
    }
  }
`;
