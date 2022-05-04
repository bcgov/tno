import styled from 'styled-components';

export const ContentListView = styled.div`
  .content-list {
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

  .box {
    margin-top: 0.6em;
    margin-left: 1em;
    border: solid 1px grey;
    border-radius: 0.25em;
    max-width: 50em;
    padding: 1em;
  }
`;
