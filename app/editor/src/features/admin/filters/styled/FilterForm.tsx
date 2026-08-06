import { FormPage } from 'components/formpage';
import styled from 'styled-components';

export const FilterForm = styled(FormPage)`
  display: flex;
  flex-direction: column;

  .form-actions {
    margin: 0.5rem 0;
    gap: 0.5rem;
    justify-content: flex-end;
  }

  .back-button {
    align-self: start;
  }

  .code {
    .editor {
      padding: 0.375rem 0.75rem;
      box-sizing: border-box;
      display: inline-block;
      font-weight: 400;
      text-align: left;
      user-select: text;
      border: 1px solid rgb(96, 96, 96);
      font-size: 1rem;
      line-height: 1.6;
      border-radius: 0.25rem;
      overflow: auto;
      /* color: rgb(0, 51, 102); */
      transition: color 0.15s ease-in-out 0s, background-color 0.15s ease-in-out 0s,
        border-color 0.15s ease-in-out 0s, box-shadow 0.15s ease-in-out 0s;
      resize: both;
      min-width: 100%;
      max-width: 100%;
      min-height: 100px;
      max-height: 500px;
    }
  }

  .results {
    max-height: 200px;
    overflow: scroll;
    border: 1px solid rgb(96, 96, 96);
    border-radius: 0.25rem;
    padding: 0.375rem 0.75rem;
  }

  hr {
    width: 100%;
  }

  .search-in {
    flex-wrap: nowrap;
    width: 100%;
  }

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

  .pad-05 {
    padding: 0.5rem;
  }

  code {
    border: solid 1px gray;
    border-radius: 0.15rem;
    padding: 0 0.25rem;
  }
`;
