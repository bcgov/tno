import styled from 'styled-components';

import { IWysiwygProps } from '../Wysiwyg';

export const Wysiwyg = styled.div<IWysiwygProps>`
  .ql-custom {
    width: 25em;
    .ql-picker-label:before,
    .ql-picker-item:before {
      content: attr(data-label);
    }
  }
  .content-menu {
    b {
      border-bottom: 1px solid;
      margin-bottom: 0.5em;
    }

    z-index: 1000;
    position: absolute;
    margin-top: 1.5em;
    background-color: white;
    border: 1px solid black;
    padding: 0.5em;
    .exit {
      margin-left: auto;
      &:hover {
        cursor: pointer;
        transform: scale(1.1);
      }
    }
    .content-option {
      &:hover {
        cursor: pointer;
        background-color: #f0f0f0;
      }
      margin-bottom: 0.25em;
    }
  }

  .add-button {
    border: 1px solid;
    padding: 0.35em 0.25em 0.25em 0.25em;
    border-radius: 0.5em;
    cursor: pointer;
    &:hover {
      background-color: #f0f0f0;
    }
  }

  padding-right: 0.5rem;
  padding-bottom: 0.5rem;

  label {
    font-weight: bold;
  }

  .required:after {
    content: ' *';
    color: ${(props) => props.theme.css.dangerColor};
    font-weight: 700;
  }

  .custom-icon {
    color: #444;
  }

  .editor {
    display: ${(props) => props.viewRaw && 'none'};
  }

  .ql-editor {
    overflow-y: scroll;
    resize: vertical;

    p {
      font-family: ${(props) => props.theme.css?.bcSans};
      font-size: 1rem;
      margin: 0.5rem 0rem;
    }
  }
  .raw-editor {
    margin: 0;
    padding: 12px 15px;
    min-height: 20rem;
    -webkit-box-sizing: border-box;
    -moz-box-sizing: border-box;
    min-width: 100%;
    box-sizing: border-box;
    resize: none;

    border: 1px solid #ccc;
    outline: none;
    display: ${(props) => (props.viewRaw ? 'block' : 'none')};
  }

  svg:hover {
    color: #3498db;
  }
`;
