import { FormPage } from 'components/formpage';
import styled from 'styled-components';

export const UserForm = styled(FormPage)`
  display: flex;
  flex-direction: column;
  align-items: center;

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
`;
