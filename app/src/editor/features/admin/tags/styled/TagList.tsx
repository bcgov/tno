import styled from 'styled-components';

export const TagList = styled.div`
  display: flex;
  /* The list is bounded by the page, never by its content - a long description
     truncates rather than widening the table and scrolling the page sideways. */
  flex: 1 1 100%;
  max-width: 100%;
  overflow-x: hidden;

  /* The page is the flex item that has to give way; left at min-width:auto it sizes to
     the table and the wrapper above would only clip it. */
  > .form-page {
    min-width: 0;
    max-width: 100%;
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
  }

  .table {
    max-height: calc(100% - 120px);
    min-height: 200px;
    min-width: 0;
    max-width: 100%;

    .header,
    .rows,
    div.row {
      max-width: 100%;
    }

    /* A flex item defaults to min-width:auto, which is what lets an unbreakable cell
       push the row wider than the table.  Every column and every cell inside one is
       allowed to shrink instead, so the ellipsis in the cell is what gives way. */
    div.column {
      min-width: 0;
      overflow: hidden;

      .ellipsis {
        flex: 1 1 0;
        min-width: 0;
      }
    }

    /* Header labels truncate with the column rather than widening it. */
    .header div.column .label {
      min-width: 0;
      overflow: hidden;
      text-overflow: ellipsis;
      white-space: nowrap;
    }

    /* Code: fixed - codes are short and the column should not move as data changes. */
    div.column.col-0 {
      flex: 0 0 8rem;
    }

    /* Name: grows with the viewport, but only so far. */
    div.column.col-1 {
      flex: 1 1 10rem;
      max-width: 20rem;
    }

    /* Description: takes whatever is left over and truncates. */
    div.column.col-2 {
      flex: 1 1 0;
    }

    /* Order and Enabled: fixed - a number and a checkbox. */
    div.column.col-3,
    div.column.col-4 {
      flex: 0 0 5rem;
    }
  }
`;
