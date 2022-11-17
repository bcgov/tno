import { FormPage } from 'components/form';
import styled from 'styled-components';

export const UserList = styled(FormPage)`
  .filter-bar {
    padding: 1.5%;
    .txt {
      margin-right: 0.5em;
    }
    .frm-in {
      padding: 0;
    }
    align-items: center;
    display: flex;
    button {
      background-color: white;
    }
    background-color: #f5f5f5;
  }
`;
