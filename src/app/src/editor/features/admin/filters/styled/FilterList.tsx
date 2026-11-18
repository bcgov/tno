import styled from 'styled-components';

export const FilterList = styled.div`
  display: flex;

  /* Aligned by the global .field-row rule (index.scss); no offsets on individual controls. */
  .filter-bar {
    padding: 0.5rem;
    background-color: #f5f5f5;

    button {
      background-color: white;
    }
  }

  div.row {
    cursor: pointer;

    div.column {
      overflow: hidden;
    }
  }

  .keyword-cell {
    display: flex;
    align-items: center;
    position: relative;

    .clipboard-icon {
      opacity: 0;
      transition: opacity 0.1s ease;
      cursor: pointer;
      margin-left: 0.5rem;
    }

    &:hover .clipboard-icon {
      opacity: 1;
    }
  }

  .grid {
    min-height: 200px;

    /* The rows scroll within the viewable area; the header and pager stay in place. Both reserve
       the scrollbar's gutter so the columns stay aligned. */
    > div > .grid-table:nth-child(1) {
      overflow-y: hidden;
      scrollbar-gutter: stable;
    }

    /* The rows fill the rest of the viewable area, however many there are, and scroll when they
       do not fit. Every row is the same compact height (text plus 0.25rem above and below, as in
       FlexboxTable); the border separates rows, and each cell centres its content. */
    > div > .grid-table:nth-child(2) {
      height: var(--grid-body-height, calc(100vh - 470px));
      overflow-y: auto;
      overflow-x: hidden;
      scrollbar-gutter: stable;
      align-content: start;
      grid-auto-rows: 2rem;
      row-gap: 0;

      > .grid-column {
        padding-top: 0;
        padding-bottom: 0;
        align-items: center;
        min-width: 0;
      }

      /* tno-core's Link pads 8px on every side, which made rows taller and indented the name. */
      .link {
        padding: 0;
      }
    }

    .grid-column {
      overflow: hidden;

      &:nth-child(5n + 5) {
        justify-content: center;
      }
    }
  }
`;
