import { FormPage } from 'components/formpage';
import styled from 'styled-components';

export const FolderForm = styled(FormPage)`
  display: flex;
  flex-direction: column;

  .form-actions {
    margin-top: 1rem;
    gap: 0.5rem;
    justify-content: center;
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

  .pad-05 {
    padding: 0.5rem;
  }

  p.note {
    background-color: ${(props) => props.theme.css.stickyNoteColor};
    padding: 0.5rem;
    margin: 0 0.5rem 0 0;
    border-bottom-left-radius: 0.5rem;
    border-bottom-right-radius: 0.5rem;
  }
`;
