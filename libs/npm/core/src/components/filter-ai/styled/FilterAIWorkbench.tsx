import styled from 'styled-components';

export const FilterAIWorkbench = styled.div`
  display: grid;
  grid-template-columns: minmax(18rem, 1fr) minmax(22rem, 1.3fr);
  grid-template-rows: minmax(0, 1fr);
  gap: 1.25rem;
  width: min(100rem, 88vw);
  height: 72vh;

  @media (max-width: 70rem) {
    grid-template-columns: 1fr;
    grid-template-rows: auto;
    height: auto;
  }

  .draft {
    display: flex;
    flex-direction: column;
    gap: 0.5rem;
    min-width: 0;
    min-height: 0;
    overflow: auto;
  }

  .actions {
    display: flex;
    flex-wrap: wrap;
    align-items: center;
    gap: 0.5rem;

    .filter-ai-button svg {
      margin-right: 0.35rem;
    }
  }

  .status {
    display: flex;
    align-items: center;
    gap: 0.5rem;
    font-size: 0.9rem;

    &.failed {
      color: #b00020;
    }
  }

  .hint {
    margin: 0;
    font-size: 0.85rem;
    color: #555;
  }

  .results {
    display: flex;
    flex-direction: column;
    gap: 0.5rem;
    min-width: 0;
    min-height: 0;
  }

  .tabs {
    display: flex;
    gap: 0.25rem;
    border-bottom: 1px solid #ddd;

    button {
      padding: 0.4rem 0.9rem;
      background: transparent;
      border: 1px solid transparent;
      border-bottom: none;
      border-radius: 0.35rem 0.35rem 0 0;
      cursor: pointer;

      &[aria-selected='true'] {
        border-color: #ddd;
        background: #fff;
        font-weight: 600;
        margin-bottom: -1px;
      }
    }
  }

  .panel {
    flex: 1 1 auto;
    min-height: 0;
    overflow: auto;
    padding: 0.5rem 0.25rem;
  }

  .explanation {
    white-space: pre-wrap;
  }

  .settings {
    display: grid;
    grid-template-columns: max-content 1fr;
    gap: 0.25rem 1rem;
    margin: 0.5rem 0;

    dt {
      font-weight: 600;
    }

    dd {
      margin: 0;
      overflow-wrap: anywhere;
    }
  }

  pre {
    margin: 0.25rem 0 0.75rem;
    padding: 0.5rem;
    background: #f6f6f6;
    border: 1px solid #eee;
    font-size: 0.8rem;
    white-space: pre-wrap;
    overflow-wrap: anywhere;
  }

  details {
    margin-bottom: 0.5rem;

    summary {
      cursor: pointer;
    }
  }

  .thread {
    display: flex;
    flex-direction: column;
    gap: 0.75rem;

    .turn {
      padding: 0.6rem 0.8rem;
      border-radius: 0.5rem;
      white-space: pre-wrap;
      overflow-wrap: anywhere;

      &.user {
        align-self: flex-end;
        background: #e8eef9;
        max-width: 85%;
      }

      &.assistant {
        align-self: flex-start;
        background: #f3f3f3;
        max-width: 95%;
      }

      .suggested {
        margin-top: 0.5rem;
        padding: 0.5rem;
        border: 1px dashed #999;
        background: #fff;
        white-space: normal;
      }
    }
  }

  .ask {
    display: flex;
    gap: 0.5rem;
    align-items: flex-end;
    margin-top: 0.75rem;

    textarea {
      flex: 1;
      min-height: 4rem;
      padding: 0.5rem;
      border: 1px solid #bbb;
      border-radius: 0.25rem;
      font: inherit;
    }
  }
`;
