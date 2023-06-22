import styled from 'styled-components';
import { ToolBarSection } from 'tno-core';

export const ContentFilter = styled(ToolBarSection)`
  .filters {
    gap: 0.5em;

    .select {
      min-width: 15em;
      max-width: unset;
    }

    .sources {
      max-width: 40em;
    }
  }

  .rs__value-container {
    max-height: 33.6px;
    overflow-y: auto;
  }
`;
