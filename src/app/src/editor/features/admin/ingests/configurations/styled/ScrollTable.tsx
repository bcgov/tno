import styled, { css } from 'styled-components';

/** Compact form fields for dense rows. */
export const compactFields = css`
  .frm-in {
    padding-bottom: 0;
    width: 100%;

    /* Selects nest the control in wrappers that otherwise size to their content. */
    & > div,
    & > div > div,
    .frm-select {
      flex: 1;
      width: 100%;
      min-width: 0;
    }
  }

  input {
    padding: 0.125rem 0.5rem;
  }

  .frm-in .rs__control {
    min-height: unset;
  }

  .frm-in .rs__value-container {
    padding: 0 0.5rem;
  }

  .frm-in .rs__input-container {
    margin: 0;
    padding: 0;
  }

  .frm-in .rs__indicator {
    padding: 0.125rem 0.375rem;
  }

  button {
    height: auto;
    padding: 0.25rem 0.5rem;
    line-height: 1;
  }
`;

/**
 * A FlexboxTable of form fields with compact rows that scroll inside the section, so a long list
 * doesn't push the rest of the form down. The header stays in place above the scrolling rows.
 */
export const ScrollTable = styled.div`
  .rows {
    max-height: 22rem;
    overflow-y: auto;
    ${compactFields}

    .row {
      margin: 0;
      padding: 0;
    }

    .column {
      padding: 0.125rem 0.25rem;
      align-items: center;

      /* Fields fill their column. */
      & > div {
        flex: 1;
        min-width: 0;
      }
    }
  }
`;
