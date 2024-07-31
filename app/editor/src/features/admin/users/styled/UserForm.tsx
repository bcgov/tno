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
`;
