import { FormPage } from 'components/form';
import styled from 'styled-components';

export const ContentListView = styled(FormPage)`
  min-height: fit-content;

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
`;
