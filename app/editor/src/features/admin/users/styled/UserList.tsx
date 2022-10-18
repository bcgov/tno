import { FormPage } from 'components/form';
import styled from 'styled-components';

export const UserList = styled(FormPage)`
  .filter-bar {
    display: flex;
    align-items: center;
    .rs__control {
      margin-top: 3.5%;
    }
    input {
      margin-top: 3.5%;
    }
    button {
      background-color: white;
    }
    background-color: #f5f5f5;
  }
`;
