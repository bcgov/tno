import styled from 'styled-components';
import { FormPage } from 'tno-core';

export const ConnectionList = styled(FormPage)`
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
