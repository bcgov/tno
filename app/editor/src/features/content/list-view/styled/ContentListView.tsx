import styled from 'styled-components';

export const ContentListView = styled.div`
  .content-list {
    table {
      background: transparent;
    }

    div[role='rowgroup'] {
      min-height: 100px;
      max-height: calc(100vh - 600px);
      overflow-y: scroll;
      overflow-x: hidden;
    }
  }

  .content-actions {
    margin-top: 1em;
    margin-bottom: 0.5em;

    button {
      display: block;
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

  .left-pane {
    display: flex;
    flex-direction: column;
    flex: 1 1 0%;
  }

  .right-pane {
    display: flex;
    flex-direction: column;
  }
`;
