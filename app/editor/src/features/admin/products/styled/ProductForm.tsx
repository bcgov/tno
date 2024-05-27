import { FormPage } from 'components/formpage';
import styled from 'styled-components';

export const ProductForm = styled(FormPage)`
  display: flex;
  flex-direction: column;

  .back-button {
    align-self: start;
  }
  align-items: center;

  .form {
    width: 100%;
  }

  .preferred {
    padding: 0 0.5rem;
    border-radius: 0.5rem;
    background: ${(props) => props.theme.css.lightAccentColor};
  }
`;
