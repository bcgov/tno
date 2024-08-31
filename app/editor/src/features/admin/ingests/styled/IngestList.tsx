import { FormPage } from 'components/formpage';
import styled from 'styled-components';

export const IngestList = styled(FormPage)`
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
    max-height: calc(100% - 150px);
    min-height: 200px;
  }

  .grid-table:nth-child(2) {
    min-height: 100px;
    max-height: calc(-290px + 100vh);
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
