import styled from 'styled-components';
import { FormPage } from 'tno-core';

export const TopicForm = styled(FormPage)`
  display: flex;
  flex-direction: column;
  .back-button {
    align-self: start;
    margin-bottom: 3%;
  }
  align-items: center;
  .form-inputs {
    margin-top: 3%;
  }
`;
