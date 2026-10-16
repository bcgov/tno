import styled from 'styled-components';

import { compactFields } from './ScrollTable';

/**
 * The syndication feeds in priority order, styled like a FlexboxTable. Rows are compact and scroll
 * inside the section below a fixed header.
 */
export const FeedList = styled.div`
  padding: 0.25rem;

  .feed-header,
  .feed-row {
    display: grid;
    /* The actions column has a set width so the header lines up with the rows. */
    grid-template-columns: 1.5rem 2rem minmax(0, 1fr) minmax(0, 1fr) 7.5rem;
    gap: 0.25rem;
    align-items: center;
  }

  .feed-header {
    font-weight: bold;
    border-top: solid 2px #000;
    border-bottom: solid 2px #000;
    margin-bottom: 0.25em;
    padding: 0.25rem;
  }

  .feed-rows {
    max-height: 22rem;
    overflow-y: auto;
    ${compactFields}
  }

  .feed-row {
    padding: 0.125rem 0.25rem;
    background-color: #fff;

    &:nth-child(even) {
      background-color: rgb(233, 236, 239);
    }

    &.dragging {
      border-radius: 0.25rem;
      box-shadow: 0 0.25rem 0.75rem rgba(0, 0, 0, 0.25);
    }
  }

  .drag-handle {
    display: flex;
    justify-content: center;
    cursor: grab;
    color: #606060;
  }
`;
