import styled from 'styled-components';
import { ToolBarSection } from 'tno-core';

export const PublishedSection = styled(ToolBarSection)`
  .view {
    cursor: pointer;
    &:hover {
      text-decoration: underline;
    }
  }
  svg {
    align-self: center;
    margin-right: 0.25rem;
  }
`;
