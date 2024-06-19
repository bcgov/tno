import { FormPage } from 'components/formpage';
import styled from 'styled-components';

export const Papers = styled(FormPage)`
  min-height: fit-content;

  .filter-select {
    margin-top: 0.25em;
  }

  hr {
    width: 100%;
    height: 0.75em;
    border: none;
    background-color: #003366;
  }

  .content-list {
    border-radius: 4px;
    box-shadow: 0px 1px 2px rgba(0, 0, 0, 0.2);
    table {
      background: transparent;
    }

    div[role='rowgroup'] {
      min-height: 100px;
      max-height: calc(100vh - 600px);
      overflow-y: overlay;
      overflow-x: hidden;
    }

    .headline {
      & > svg:first-child {
        margin-right: 0.5em;
      }
    }
  }

  .content-actions {
    button {
      display: block;
    }
  }

  .top-pane {
    display: flex;
    flex-direction: column;
    flex: 1 1 0%;
  }

  .bottom-pane {
    display: flex;
    flex-direction: column;
    flex: 1 1 0%;
  }

  .h-status {
    text-align: center;
    align-items: center;
    justify-content: center;
  }

  .h-publishedOn {
    text-align: center;
    align-items: center;
    justify-content: center;
  }

  .h-use {
    text-align: center;
    align-items: center;
    justify-content: center;
  }

  .table {
    width: 100%;

    .rows {
      min-height: 100px;
      max-height: calc(-450px + 100vh);
    }

    .column {
      overflow: hidden;
    }

    .row:hover {
      cursor: pointer;
    }
  }

  .paper-totals {
    display: flex;
    flex-direction: row;
    gap: 2rem;
    justify-content: center;

    > div {
      display: flex;
      flex-direction: row;
      gap: 0.5rem;
      background: lightgray;
      border-radius: 0.25rem;
      padding: 0.15rem 0.5rem;

      > div:last-child {
        background: white;
        border-radius: 0.25rem;
        padding: 0 0.15rem;
      }
    }
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
      &.active {
        background-color: rgb(66, 139, 202);
        color: #fff;
        position: relative;
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
