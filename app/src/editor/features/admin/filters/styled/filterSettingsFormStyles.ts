import { css } from 'styled-components';

/** Styles for the fields of FilterSettingsForm, shared by the pages and modals that render it. */
export const filterSettingsFormStyles = css`
  /* Sources field + its "NOT" toggle grouped so the multi-select fills the row
   and the toggle lines up with the select input (not the "Sources" label). */
  .not-sources {
    display: flex;
    align-items: flex-start;
    gap: 0.5rem;
    width: 100%;

    .sources-select {
      flex: 1;
    }

    /* The toggle sits under an invisible spacer label of the same height as the
     "Sources" label, so both controls start on the same row. */
    .not-toggle-field {
      display: flex;
      flex-direction: column;

      .not-spacer {
        visibility: hidden;
        user-select: none;
      }
    }

    /* Style the toggle like the neighbouring input: bordered box matching the
     react-select control height, with a green highlight when active. */
    .button-toggle.not-toggle {
      height: 38px;
      box-sizing: border-box;
      padding: 0 0.75rem;
      border: 1px solid rgb(96, 96, 96);
      border-radius: 0.25rem;
      background-color: #ffffff;
      align-items: center;
      transition: background-color 0.15s ease-in-out, border-color 0.15s ease-in-out;

      label {
        font-weight: 600;
      }

      &.active {
        background-color: #d4edda;
        border-color: #28a745;
      }
    }
  }
`;
