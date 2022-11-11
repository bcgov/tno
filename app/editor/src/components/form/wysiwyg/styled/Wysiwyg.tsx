import styled from 'styled-components';

export const Wysiwyg = styled.div`
  margin-bottom: 1rem;
  margin-top: 1rem;
  .custom-icon {
    color: #444;
  }
  .ql-editor {
    min-height: 250px;
  }
  .raw-editor {
    width: 99.5%;
    min-height: 250px;
    border: 1px solid #ccc;
    display: block;
  }

  svg: hover {
    color: #3498db;
  }
`;
