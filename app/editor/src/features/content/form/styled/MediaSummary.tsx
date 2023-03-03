import styled from 'styled-components';
import { Row } from 'tno-core';

export const MediaSummary = styled(Row)`
  justify-content: space-evenly;
  margin-bottom: 1rem;
  margin-top: 1rem;
  hr {
    height: 0.05em;
  }
  .summary {
    width: 60%;
    .ql-editor {
      min-height: 18rem;
    }
    .raw-editor {
      min-height: 18rem;
    }
  }
`;
