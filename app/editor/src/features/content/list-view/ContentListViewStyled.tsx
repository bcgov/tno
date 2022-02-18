import styled from 'styled-components';

export const ContentListView = styled.div`
  background-color: white;
  max-width: fit-content;
  height: 100%;
  padding: 5%;
  input[type='text'],
  select {
    min-width: 15em;
  }

  .content-filter {
    display: flex;
    flex-direction: row;
    margin-bottom: 1em;

    h2 {
      border: 0;
      padding: 0;
      margin-top: 0;
      font-weight: 400;
      font-size: 1em;
    }

    & > div:first-child {
      max-width: 20em;
      margin-right: 1em;
    }

    & > div:nth-child(2) {
      flex-grow: 2;

      & > div:nth-child(2) {
        display: flex;
        flex-direction: row;
      }
    }

    .dateRange {
    }

    .action-filters {
      display: flex;
      flex-direction: row;
    }
  }

  .content-list {
    max-width: fit-content;
    table {
      background: transparent;
    }
  }

  .content-actions {
    margin-top: 1em;

    button {
      display: block;
    }

    .addition-actions {
      margin-top: 1em;
      button {
        margin-bottom: 0.5em;
      }
    }
  }
`;
