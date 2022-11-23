import { FormPage } from 'components/form/formpage/styled';
import styled from 'styled-components';

export const WorkOrderForm = styled(FormPage)`
  display: flex;
  flex-direction: column;
  align-items: center;

  .back-button {
    align-self: start;
    margin-bottom: 3%;
  }

  .form-inputs {
    margin-top: 3%;
  }
`;
