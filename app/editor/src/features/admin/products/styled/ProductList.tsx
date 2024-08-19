import styled from 'styled-components';

export const ProductList = styled.div`
  display: flex;
  flex-flow: column;
  flex-grow: 1;

  .filter-bar {
    display: flex;
    align-items: center;
    background-color: #f5f5f5;
    align-items: center;
    gap: 1rem;

    input {
      margin-top: 3.5%;
    }
    button {
      background-color: white;
    }

    .frm-in {
      padding: 0;
      input {
        margin-top: 0;
      }
    }
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
