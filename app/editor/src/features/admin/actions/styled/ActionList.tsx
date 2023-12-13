import styled from 'styled-components';

export const ActionList = styled.div`
  display: flex;
  flex-flow: column;
  flex-grow: 1;
  .form-page {
    display: flex;
    flex-flow: column;
  }

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

  .table {
    // max-height: calc(100% - 120px);
    min-height: 200px;
  }
`;
