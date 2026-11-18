import styled from 'styled-components';

export const SettingList = styled.div`
  width: 100%;
  height: 100%;
  min-height: 100%;
  display: flex;
  justify-content: center;

  .history-retention {
    margin: 0.5rem 0 1rem 0;
    padding: 0.5rem;
    border: 1px solid ${(props) => props.theme.css.lightVariantColor};
    border-radius: 0.25rem;

    .purge-counts {
      margin-top: 0.5rem;
    }
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
    max-height: calc(100% - 120px);
    min-height: 200px;
  }
`;
