import styled from 'styled-components';

export const GridTable = styled.div`
  display: grid;
  padding: 5px;
  border-radius: 0.25em;
  grid-auto-rows: auto;
  gap: 0.25em;

  div[role='rowheader'] {
    div[role='row'] {
      &:first-child {
        background-color: transparent;
        border-top: 2px solid #606060;
        border-bottom: 2px solid #606060;
      }
    }
  }

  div[role='row'] {
    display: grid;
    grid-auto-flow: column;
    grid-auto-columns: 1fr;
    border-bottom: solid 1px #efefef;

    &:not(.rh):hover {
      background: linear-gradient(rgba(0, 0, 0, 0.15), rgba(0, 0, 0, 0.15));
    }

    &:nth-child(even) {
      background-color: ${(props) => props.theme.css.tableEvenRowColor ?? '#fff'};
    }

    &:nth-child(odd) {
      background-color: ${(props) => props.theme.css.tableOddRowColor ?? '#fff'};
    }
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
