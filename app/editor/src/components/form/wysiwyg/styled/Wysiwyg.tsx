import styled from 'styled-components';

interface IWysiwygProps {
  viewRaw: boolean;
}

export const Wysiwyg = styled.div<IWysiwygProps>`
  margin-bottom: 1rem;
  margin-top: 1rem;
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
    min-height: 250px;
  }
  .raw-editor {
    width: 99.5%;
    min-height: 250px;
    border: 1px solid #ccc;
    display: block;
    outline: none;
    display: ${(props) => (props.viewRaw ? 'block' : 'none')};
  }

  svg: hover {
    color: #3498db;
  }
`;
