import { FormPage } from 'components/form';
import styled from 'styled-components';

export const ContentListView = styled(FormPage)`
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
    width: 50%;
  }

  .right-pane {
    display: flex;
    flex-direction: column;
    flex: 1 1 0%;
  }

  min-height: fit-content;
`;
