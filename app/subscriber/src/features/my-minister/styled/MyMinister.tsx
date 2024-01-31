import styled from 'styled-components';

export const MyMinister = styled.div`
  .table {
    width: 100%;
    .group {
      background-color: #f9f9f9 !important;
    }
    .header {
      background-color: #f5f6fa;
      /* box shadow only on bottom */
      box-shadow: 0 2px 4px 0 rgba(0, 0, 0, 0.1);
      border: none;
      color: #7c7e8a;

      .column {
        background-color: #f5f6fa;
      }
    }
    .rows {
      cursor: pointer;
    }
  }
  // tone column
  .column.col-1 {
    width: 2.5rem;
    flex: unset;
    div {
      width: 100%;
      justify-content: center;
      svg.tone-icon {
        margin-left: unset;
      }
    }
  }

  .headline {
    /* link color */
    color: #3847aa;
  }
  .tableHeadline {
    width: 100%;
    table-layout: fixed;
  }

  .dateColumn {
    width: 20%;
  }

  .headlineColumn {
    width: 60%;
  }

  .mentionsColumn {
    width: 20%;
  }

  .td-date {
    white-space: nowrap;
  }

  .ministerCheckboxes {
    display: flex;
  }

  .option {
    margin: 5px;
    font-weight: bold;
  }

  .mentions {
    display: flex;
  }

  .mentionTag {
    display: flex;
    border-radius: 4px;
    background: ${(props) => props.theme.css.tagBackgroundColor};
    padding: 4px 4px;
    justify-content: center;
    align-items: center;
    gap: 2px;
    color: #000;
    text-align: center;
    font-family: ${(props) => props.theme.css.fPrimary};
    font-size: 12px;
    font-style: normal;
    font-weight: 600;
    line-height: 12px; /* 100% */
    text-transform: uppercase;
    margin-right: 6px;
  }
`;
