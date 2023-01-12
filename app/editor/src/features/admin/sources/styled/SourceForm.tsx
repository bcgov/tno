import styled from 'styled-components';
import { FormPage } from 'tno-core';

export const SourceForm = styled(FormPage)`
  .info {
    p {
      margin-left: 1em;
    }
  }
  .back-button {
    margin-bottom: 2em;
  }
  form > div {
    gap: 1em;
  }
  .form-actions {
    padding: 0.5em;
  }
  .status {
    max-width: 14em;
    padding: 0 1em 0 1em;
  }
`;
