import styled from 'styled-components';

export const ChartList = styled.div`
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

  .table {
    max-height: calc(100% - 120px);
    min-height: 200px;
    max-width: 100vw;
  }
`;
