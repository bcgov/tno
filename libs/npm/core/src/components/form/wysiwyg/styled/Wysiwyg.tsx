import styled from 'styled-components';

import { IWysiwygProps } from '../Wysiwyg';

export const Wysiwyg = styled.div<IWysiwygProps>`
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
