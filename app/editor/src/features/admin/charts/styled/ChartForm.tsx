import { FormPage } from 'components/formpage';
import styled from 'styled-components';

export const ChartForm = styled(FormPage)`
  display: flex;
  flex-direction: column;
  align-items: center;

  .back-button {
    align-self: start;
  }

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
    max-height: 500px;
  }

  .form {
    width: 100%;

    .required::after {
      content: ' *';
      color: rgb(216, 41, 47);
    }
  }

  .preview-image {
    height: 100%;
    border: solid 2px ${(props) => props.theme.css.primaryColor};
    border-radius: 0.5rem;
    display: flex;
    flex-flow: row;
    justify-content: center;
    box-shadow: 0 3px 15px rgb(0 0 0 / 0.5);
    margin-top: 1rem;
  }

  .enable-options {
    & .chk {
      margin-right: 1em;
    }
  }

  hr {
    width: 100%;
  }

  .icon-button {
    padding: 0.15rem;
    color: ${(props) => props.theme.css.primaryColor};
    border: solid 1px ${(props) => props.theme.css.primaryColor};
    border-radius: 0.25rem;

    &:hover {
      color: ${(props) => props.theme.css.primaryLightColor};
      border: solid 1px ${(props) => props.theme.css.primaryLightColor};
      box-shadow: 1px 1px ${(props) => props.theme.css.primaryColor};
      cursor: pointer;
    }
  }
`;
