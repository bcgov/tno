import styled from 'styled-components';
import { Row } from 'tno-core';

export const MediaSummary = styled(Row)`
  justify-content: space-evenly;
  flex-wrap: row;

  hr {
    height: 0.05em;
  }

  .summary {
    width: 65%;
    .ql-editor {
      min-height: 18rem;
    }
    .raw-editor {
      min-height: 18rem;
    }
  }
`;
