import styled from 'styled-components';
import { Row } from 'tno-core';

export const MediaSummary = styled(Row)`
  flex-wrap: wrap-reverse;

  hr {
    height: 0.05em;
  }

  .summary {
    flex: 1;
    display: flex;
    flex-flow: column;
    padding-right: 0.5rem;
    min-width: 500px;

    .quill {
      flex: 1;
    }
  }

  .media {
    flex: 0 0 0;
    justify-content: center;
    margin-left: auto;
    margin-right: auto;
    max-height: 350px;

    video {
      max-height: 300px;
    }
  }
`;
