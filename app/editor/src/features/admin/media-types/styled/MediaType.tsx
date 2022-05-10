import { FormPage } from 'components/form/formpage/styled';
import styled from 'styled-components';

export const MediaType = styled(FormPage)`
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
