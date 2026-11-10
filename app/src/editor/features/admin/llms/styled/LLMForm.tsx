import { FormPage } from '@/editor/components/formpage';
import styled from 'styled-components';

export const LLMForm = styled(FormPage)`
  display: flex;
  flex-direction: column;
  align-items: center;

  /* Every tab is the same size: the page fills the window and the tab body scrolls within it. */
  flex: 1 1 0;
  min-height: 0;

  .back-button {
    align-self: start;
    flex-shrink: 0;
  }

  > .form {
    align-self: stretch;
  }

  > .form,
  > .form > .box,
  > .form > .box > form,
  > .form > .box > form > .tabs,
  .tab-container,
  .form-container {
    display: flex;
    flex-direction: column;
    flex: 1 1 0;
    min-height: 0;
  }

  .form-container {
    overflow-y: auto;
  }

  .form-inputs {
    flex-shrink: 0;
    margin-left: auto;
    margin-right: auto;
  }

  .llm-actions {
    flex-shrink: 0;
    position: sticky;
    bottom: 0;
    margin-top: auto;
    padding: 0.5rem 0;
    background-color: ${(props) => props.theme.css.formBackgroundColor};
  }

  .llm-prompt {
    display: flex;
    flex-direction: column;
    flex: 1 1 0;
    min-height: 15em;

    /* Quill sizes itself from its container, so it is pinned to the space the flex layout gives.
       The shared component toggles .editor's display for the raw view; don't set it here. */
    .editor {
      position: relative;
      flex: 1 1 0;
      min-height: 0;
    }

    .ql-container {
      position: absolute;
      inset: 0;
      height: auto;
    }

    .ql-editor {
      height: 100%;
      overflow-y: auto;
      resize: none;
    }

    .raw-editor {
      flex: 1 1 0;
      min-height: 0;
      overflow-y: auto;
    }
  }

  .llm-usage {
    flex-shrink: 0;
    display: flex;
    flex-direction: column;
    gap: 1rem;
    padding: 1rem;

    div.row {
      cursor: pointer;
    }

    .llm-usage-messages {
      margin: 0;
      padding-left: 1.25rem;

      &.errors {
        color: ${(props) => props.theme.css.dangerColor};
      }

      &.warnings {
        color: ${(props) => props.theme.css.warningColor};
      }
    }
  }
`;
