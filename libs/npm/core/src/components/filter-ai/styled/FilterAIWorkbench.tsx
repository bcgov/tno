import styled from 'styled-components';

export const FilterAIWorkbench = styled.div`
  display: grid;
  grid-template-columns: minmax(20rem, 1fr) minmax(24rem, 1.4fr);
  grid-template-rows: minmax(0, 1fr);
  gap: 1.25rem;
  width: min(110rem, 92vw);
  height: 78vh;

  @media (max-width: 70rem) {
    grid-template-columns: 1fr;
    grid-template-rows: auto auto;
    height: auto;

    .stories .list {
      max-height: 20rem;
    }
  }

  .draft {
    display: flex;
    flex-direction: column;
    gap: 0.5rem;
    min-width: 0;
    min-height: 0;

    .filter-ai-prompt-editor textarea {
      min-height: 7rem;
    }
  }

  .actions {
    display: flex;
    flex-wrap: wrap;
    align-items: center;
    gap: 0.75rem;

    .filter-ai-button svg {
      margin-right: 0.35rem;
    }

    .published-only {
      display: flex;
      align-items: center;

      > div {
        display: flex;
        align-items: center;
        gap: 0.35rem;
        cursor: pointer;
      }

      svg {
        width: 1.5rem;
        height: 1.5rem;
      }

      label {
        margin: 0;
        cursor: pointer;
        font-size: 0.9rem;
      }
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

  .stories {
    flex: 1 1 auto;
    display: flex;
    flex-direction: column;
    min-height: 8rem;
    border: 1px solid #ddd;
    border-radius: 0.35rem;
    overflow: hidden;

    .stats {
      display: flex;
      flex-wrap: wrap;
      align-items: center;
      gap: 0.35rem 0.75rem;
      padding: 0.5rem 0.75rem;
      background: #f6f6f6;
      border-bottom: 1px solid #ddd;
      font-size: 0.85rem;

      .count {
        display: flex;
        align-items: center;
        gap: 0.35rem;
        margin-right: 0.25rem;
      }

      .stat {
        padding: 0.1rem 0.5rem;
        border: 1px solid #ddd;
        border-radius: 1rem;
        background: #fff;
        white-space: nowrap;
      }

      .sentiment-stat {
        display: inline-flex;
        align-items: center;
        gap: 0.2rem;

        .sentiment {
          display: inline-flex;
          margin-left: 0.3rem;
        }
      }
    }

    .status {
      padding: 0.5rem 0.75rem;
    }

    .selected-story {
      display: flex;
      flex-wrap: wrap;
      align-items: center;
      gap: 0.5rem;
      padding: 0.4rem 0.75rem;
      border-bottom: 1px solid #ddd;
      background: #eef2fb;
      font-size: 0.85rem;

      .label {
        font-weight: 600;
      }

      .headline {
        flex: 1 1 10rem;
        min-width: 0;
        overflow: hidden;
        text-overflow: ellipsis;
        white-space: nowrap;
      }
    }

    .list {
      flex: 1 1 auto;
      min-height: 0;
      overflow: auto;
      margin: 0;
      padding: 0;
      list-style: none;

      li {
        display: grid;
        grid-template-columns: max-content max-content minmax(0, 1fr) max-content;
        align-items: center;
        gap: 0.75rem;
        padding: 0.35rem 0.75rem;
        border-bottom: 1px solid #eee;
        font-size: 0.85rem;

        &:nth-child(even) {
          background: #fafafa;
        }

        cursor: pointer;

        &:hover {
          background: #f0f4fb;
        }

        &.selected {
          background: #e8eef9;
          box-shadow: inset 3px 0 0 #3b5bdb;
        }

        &:focus-visible {
          outline: 2px solid #3b5bdb;
          outline-offset: -2px;
        }

        .published {
          font-variant-numeric: tabular-nums;
          color: #555;
          white-space: nowrap;
        }

        .source {
          font-weight: 600;
          white-space: nowrap;
        }

        .headline {
          overflow: hidden;
          text-overflow: ellipsis;
          white-space: nowrap;
        }

        .sentiment {
          display: inline-flex;
        }
      }
    }
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

    &.query {
      display: flex;
      flex-direction: column;
      gap: 0.5rem;
      overflow: hidden;

      .json-editor {
        flex: 1 1 auto;
        min-height: 16rem;
      }
    }

    &.manual .manual-filter {
      margin-top: 0.5rem;
    }
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

  .thread .turn .about {
    margin-bottom: 0.25rem;
    font-size: 0.8rem;
    color: #555;
  }

  .about-story {
    display: flex;
    flex-direction: column;
    gap: 0.4rem;
    margin-top: 0.75rem;
    padding: 0.5rem 0.75rem;
    border: 1px dashed #99a;
    border-radius: 0.35rem;
    background: #eef2fb;
    font-size: 0.9rem;

    .quick {
      display: flex;
      flex-wrap: wrap;
      align-items: center;
      gap: 0.5rem;
    }
  }

  button.link {
    padding: 0;
    border: none;
    background: none;
    color: #3b5bdb;
    font: inherit;
    font-size: 0.85rem;
    text-decoration: underline;
    cursor: pointer;
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
