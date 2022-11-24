import styled from 'styled-components';

export const GridTable = styled.div`
  display: grid;
  padding: 5px;
  border-radius: 0.25em;
  grid-auto-rows: auto;
  gap: 0.25em;
  width: 100%;

  .center {
    text-align: center;
  }

  div[role='rowheader'] {
    .filterable {
      margin-bottom: 0.25em;
    }
    div[role='row'] {
      &:first-child {
        background-color: transparent;
        border-top: 2px solid #606060;
        border-bottom: 2px solid #606060;
      }
    }
  }

  div[role='row'] {
    position: relative;
    display: grid;
    grid-auto-flow: column;
    grid-auto-columns: 1fr;
    border-bottom: solid 1px ${(props) => props.theme.css.borderColor};
    &.selected {
      background-color: ${(props) => props.theme.css.secondaryVariantColor};
      color: ${(props) => props.theme.css.backgroundColor};
    }
    &.active {
      background-color: ${(props) => props.theme.css.primaryLightColor};
      color: ${(props) => props.theme.css.backgroundColor};
    }

    &:nth-child(even):not(.active):not(.selected) {
      background-color: ${(props) =>
        props.theme.css.tableEvenRowColor ?? props.theme.css.backgroundColor};
    }

    &:nth-child(odd):not(.active):not(.selected) {
      background-color: ${(props) =>
        props.theme.css.tableOddRowColor ?? props.theme.css.backgroundColor};
    }
  }

  div[role='columnheader'] {
    display: flex;
    font-weight: bold;
    padding: 0.25em;
  }

  div[role='rowgroup'] {
    position: relative;
    display: grid;
    grid-auto-rows: 1fr;

    div[role='row'] {
      &:nth-child(even) {
        background-color: ${(props) => props.theme.css.tableRowNColor};
      }
      &:hover {
        /* background-color: ${(props) => props.theme.css.tableRowHoverColor}; */
        filter: brightness(50%);
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
