import styled from 'styled-components';

export const GridTable = styled.div`
  display: grid;
  background: ${(props) => props.theme.css.tableColor};
  padding: 5px;
  border-radius: 0.25em;
  grid-auto-rows: auto;
  gap: 0.25em;

  div[role='rowheader'] {
    div[role='row'] {
      border-top-left-radius: 0.25em;
      border-top-right-radius: 0.25em;

      &:first-child {
        background-color: ${(props) => props.theme.css.tableHeaderColor};
      }
    }
  }

  div[role='row'] {
    display: grid;
    grid-auto-flow: column;
    grid-auto-columns: 1fr;
  }

  div[role='columnheader'] {
    font-weight: bold;
    padding: 0.25em;
  }

  div[role='rowgroup'] {
    display: grid;
    grid-auto-rows: 1fr;

    div[role='row'] {
      &:nth-child(even) {
        background-color: ${(props) => props.theme.css.tableRowNColor};
      }
      &:hover {
        background-color: ${(props) => props.theme.css.tableRowHoverColor};
        cursor: pointer;
      }
      &:last-child {
        border-bottom-left-radius: 0.25em;
        border-bottom-right-radius: 0.25em;
      }
    }
  }

  div[role='cell'] {
    display: grid;
    grid-auto-flow: column;
    grid-auto-columns: 1fr;
    padding: 0.25em;

    input[type='checkbox'] {
      width: 2em;
    }
  }
`;

export default GridTable;
