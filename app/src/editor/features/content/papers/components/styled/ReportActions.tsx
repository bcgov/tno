import styled from 'styled-components';
import { Row } from 'tno-core';

export const ReportActions = styled(Row)`
  margin-top: 1em;

  hr {
    width: 2px;
    height: 100%;
    margin: 0 0 0.25em;
  }

  .separate-buttons {
    padding: 0 1em 0 0.75em;
    margin: 0;
  }

  .separate-actions {
    padding: 0 0.75em 0 0.25em;
    margin: 0;
  }

  .icon {
    align-self: center;
    padding: 0.25em 0.25em 0 0;
    color: #bcbec5;
    height: 2em;
    width: 1.25em;
  }
`;
