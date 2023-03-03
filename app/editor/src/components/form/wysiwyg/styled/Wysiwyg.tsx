import styled from 'styled-components';

import { IWysiwygProps } from '../Wysiwyg';

export const Wysiwyg = styled.div<IWysiwygProps>`
  margin-bottom: 1rem;
  width: 100%;

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
    min-height: 25rem;
    height: ${(props) => props.hasHeight && '43rem'};
    font-family: ${(props) => props.theme.css?.bcSans};
    font-size: 1rem;
  }

  .quill {
    border: 1px solid #ccc;
  }

  .ql-container {
    border: none;
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

  p {
    margin-bottom: 1em;
  }
`;
