import { FormPage } from 'components/formpage';
import styled from 'styled-components';

export const ReportTemplateForm = styled(FormPage)`
  display: flex;
  flex-direction: column;
  align-items: center;

  .back-button {
    align-self: start;
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
      min-width: 100%;
      max-width: 100%;
    }
  }

  .form {
    width: 100%;

    .required::after {
      content: ' *';
      color: rgb(216, 41, 47);
    }

    .frm-in.chk {
      margin-right: 1em;
    }
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

  .enable-options {
    & .chk {
      margin-right: 1em;
    }
  }

  hr {
    width: 100%;
  }

  .add-chart {
    .frm-in {
      min-width: 50%;
    }
    button {
      align-self: flex-end;
      margin-bottom: 0.5rem;
    }
  }

  .charts {
    padding: 0.5rem;
    border: solid 1px ${(props) => props.theme.css.tableEvenRowColor};
    border-radius: 0.25rem;
    gap: 0.25rem;

    & div[direction='row'] {
      border-radius: 0.25rem;
      align-items: center;
      padding: 0.25rem;
    }

    & div[direction='row']:nth-child(odd) {
      background-color: ${(props) => props.theme.css.tableOddRowColor};
    }
    & div[direction='row']:nth-child(even) {
      background-color: ${(props) => props.theme.css.tableEvenRowColor};
    }
  }

  .error {
    color: red;
  }

  .preview-body {
    h1,
    h2,
    h3,
    h4,
    h5 {
      text-transform: none;
    }
  }
`;
