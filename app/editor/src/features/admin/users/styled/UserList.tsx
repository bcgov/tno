import { FormPage } from 'components/formpage';
import styled from 'styled-components';

export const UserList = styled(FormPage)`
  .export-button-container {
    display: flex;
    align-items: center;
    border: none;
  }
  .export-button {
    background-color: white;
    color: #04814d;
    margin-left: 1em;
    border-radius: 6px;
    border: 1px solid #04814d;
    font-weight: normal;
  }
  .export-button:hover {
    color: #04814d;
  }
  .export-button-icon {
    width: 20px;
    height: 20px;
  }

  .filter-bar {
    padding: 1.5%;
    .txt {
      margin-right: 0.5em;
    }
    .frm-in {
      padding: 0;
    }
    align-items: center;
    display: flex;
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

  .grid-table:nth-child(2) {
    min-height: 100px;
    max-height: calc(-450px + 100vh);
    overflow: auto;
    margin-right: -17px;

    .grid-column {
      > .clickable {
        cursor: pointer;
      }
      > div {
        width: 100%;
      }
    }
  }

  .grid {
    width: 100%;
  }
`;
