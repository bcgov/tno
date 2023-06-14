import styled from 'styled-components';
import { FormPage } from 'tno-core';

export const UserForm = styled(FormPage)`
  display: flex;
  flex-direction: column;
  align-items: center;

  .back-button {
    align-self: start;
    margin-bottom: 3%;
  }

  .roles {
    border: solid 1px grey;
    border-radius: 0.25em;
    padding: 0 0.5em 0.5em 0.5em;
  }
`;
