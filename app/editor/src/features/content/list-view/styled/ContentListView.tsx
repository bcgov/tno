import { FormPage } from 'components/form';
import styled from 'styled-components';

export const ContentListView = styled(FormPage)`
  min-height: fit-content;

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
      overflow-y: scroll;
      overflow-x: hidden;
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
`;
