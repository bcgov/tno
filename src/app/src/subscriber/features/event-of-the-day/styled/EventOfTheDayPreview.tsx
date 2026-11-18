import styled from 'styled-components';

export const EventOfTheDayPreview = styled.div`
  max-width: 100%;
  min-width: calc(100% - 1.5rem);
  margin: 1rem;

  .danger {
    margin-bottom: 0.5em;
    color: #dc3545;
  }

  h1 {
    font-size: 1.5rem;
    text-transform: unset;
  }

  .section-header {
    background-color: #ddd6c8;
    color: #4d4f5c;
  }

  table {
    /* border-color: #ddd6c8; */
    border: none;
    thead {
      background-color: #f5f6fa;
      tr {
      }
      td {
        border: none;
      }
    }
    tr {
      border-color: #ddd6c8;
      outline: none;
      color: #4d4f5c;
      td {
        border-color: #ddd6c8;
        padding: 0.5em;
      }
    }
  }

  .buttons {
    margin-left: auto;
  }

  .preview-report {
    align-items: stretch;
    margin: 2rem;

    > .preview-subject {
      flex: 1;
      max-width: calc(100% - 2rem);
      background-color: white;
      padding: 1rem;
      border-top-right-radius: 0.5rem;
      border-top-left-radius: 0.5rem;
      border-bottom: solid 1px black;
      box-shadow: 10px 5px 10px grey;
    }

    > .preview-body {
      flex: 1;
      max-width: calc(100% - 2rem);
      background-color: white;
      padding: 1rem;
      border-bottom-right-radius: 0.5rem;
      border-bottom-left-radius: 0.5rem;
      box-shadow: 10px 5px 10px grey;

      img {
        max-width: 80%;
      }
    }
  }
`;
