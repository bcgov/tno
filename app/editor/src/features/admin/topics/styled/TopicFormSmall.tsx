import { FormPage } from 'components/formpage';
import styled from 'styled-components';

export const TopicFormSmall = styled(FormPage)`
  display: flex;
  flex-direction: column;
  align-items: center;
  position: relative;

  .back-button {
    align-self: start;
  }
  .topic-type-toggle-group {
    margin-right: 0.5rem;
    div {
      padding: 0;
    }
    div button.toggle-item {
      height: unset;
      padding: 8px;
      font-size: inherit;
    }
  }
  div.form {
    width: 100%;
  }
  form {
    background-color: lightgrey;
    padding: 5px;
  }
`;
