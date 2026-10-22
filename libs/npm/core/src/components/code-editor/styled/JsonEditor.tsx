import styled from 'styled-components';

export const JsonEditor = styled.div`
  display: flex;
  flex-direction: column;
  min-height: 0;
  border: 1px solid #bbb;
  border-radius: 0.25rem;
  overflow: hidden;

  > section {
    flex: 1 1 auto;
    min-height: 0;
  }

  .monaco-editor {
    --vscode-editor-background: #fff;
  }
`;
