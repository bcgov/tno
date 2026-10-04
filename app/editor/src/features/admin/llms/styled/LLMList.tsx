import styled from 'styled-components';

export const LLMList = styled.div`
  width: 100%;
  height: 100%;
  min-height: 100%;
  display: flex;
  justify-content: center;

  .filter-bar {
    display: flex;
    align-items: center;
    input {
      margin-top: 3.5%;
    }
    button {
      background-color: white;
    }
    background-color: #f5f5f5;
  }

  div.row {
    cursor: pointer;

    div.column {
      overflow: hidden;
    }
  }

  .actions {
    display: flex;
    gap: 0.75rem;
    justify-content: center;
  }

  .action-button {
    border: 0;
    background: transparent;
    padding: 0;
    display: inline-flex;
    align-items: center;
    cursor: pointer;
    color: #475467;
    font-size: 1rem;
    line-height: 1;
  }

  .action-button:hover:not(:disabled) {
    color: #0f172a;
  }

  .action-button.delete:hover:not(:disabled) {
    color: #b42318;
  }

  .action-button:disabled {
    opacity: 0.45;
    cursor: not-allowed;
  }

  .table {
    max-height: calc(100% - 120px);
    min-height: 200px;
  }
`;
