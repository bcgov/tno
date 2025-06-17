import { FormPage } from 'components/formpage';
import styled from 'styled-components';

export const UserForm = styled(FormPage)`
  display: flex;
  flex-direction: column;
  align-items: center;

  .info {
    padding: 2rem;
    background-color: red;
    color: white;
  }

  .back-button {
    align-self: start;
  }

  .no-border {
    border: none;
    padding: 0;
    margin: 0;
  }

  .button-actions {
    margin-left: 0.2em;
    margin-right: 0.2em;
  }

  .info-bar {
    width: 100%;
    height: 100%;
    display: flex;
    justify-content: space-between;
    align-items: center;

    .info-bar-icon {
      color: #f8bb47;
      width: 22px;
      height: 22px;
    }
    .info-bar-header {
      padding-left: 0.5em;
      font-weight: bold;
    }
    .info-bar-button {
      background-color: white;
      text-align: center;
      display: inline-block;
      margin-right: 1em;
    }
    background-color: #fef1d8;
    margin-top: 0.5em;
    margin-bottom: 0.5em;
    padding: 1em;
  }

  .distribution-list {
    > div:first-child {
      align-items: flex-end;

      button {
        margin-bottom: 0.5rem;
      }
    }

    .addresses {
      width: calc(100% - 1rem);
      max-height: 500px;
      overflow-y: scroll;

      > div {
        padding: 0.25rem;
        gap: 0.25rem;

        > div {
          padding: 0.25rem;
          background-color: ${(props) => props.theme.css.beigeBackgroundColor};
          border-radius: 0.25rem;
        }
      }
    }
  }

  .transfer-objects {
    margin-top: 1rem;
    display: flex;
    flex-direction: column;
    gap: 1rem;

    .grid-section {
      display: flex;
      flex-direction: column;

      > label:first-child {
        flex: 1;
        border-bottom: solid 1px black;
      }

      .header {
        font-weight: 500;
      }

      > div {
        display: grid;
        grid-template-columns: 40px 2fr 1fr 2fr;
        padding: 0.25rem;
        row-gap: 0.25rem;

        > div {
          display: flex;
          flex-direction: row;
          align-items: center;
          min-height: 2rem;
        }

        > div:nth-child(8n + 1),
        > div:nth-child(8n + 2),
        > div:nth-child(8n + 3),
        > div:nth-child(8n + 4) {
          background: ${(props) => props.theme.css.tableEvenRow};
        }
      }

      .frm-in {
        padding: 0;
        margin: 0;

        input.txt {
          padding: 0 0.25rem;
        }
      }

      .frm-in:nth-child(4n + 4) {
        flex: 1;
      }

      .error {
        input[required] {
          color: ${(props) => props.theme.css.dangerColor};
          border-color: ${(props) => props.theme.css.dangerColor};
        }
      }
    }
  }

  .form {
    width: 100%;

    .form-container {
      display: flex;
      flex-direction: column;
      align-items: center;

      > div {
        min-width: 50%;
      }
    }
  }

  .subscriber-list {
    display: flex;
    flex-direction: column;

    > div {
      flex: 1;
    }
  }
`;
