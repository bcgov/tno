import styled from 'styled-components';

export const TopicScoreAdmin = styled.div`
  width: 100%;

  h2 {
    font-size: 1.1rem;
    margin: 0.5rem 0;
  }

  .hint {
    color: ${(props) => props.theme.css.lightVariantColor};
    font-size: 0.9rem;
  }

  .warning {
    color: ${(props) => props.theme.css.dangerColor};
  }

  .page-description {
    margin-bottom: 1rem;
  }

  .panes {
    display: grid;
    grid-template-columns: minmax(18rem, 1fr) 3fr;
    gap: 1rem;
    align-items: start;
    margin-bottom: 2rem;
  }

  .sources-pane {
    display: flex;
    flex-direction: column;
    gap: 0.25rem;
  }

  .sources-header,
  .source-row {
    display: grid;
    grid-template-columns: 1fr 4ch 7ch;
    gap: 0.5rem;
    align-items: center;
  }

  .sources-header {
    font-weight: 600;
    border-bottom: 1px solid ${(props) => props.theme.css.lightVariantColor};
  }

  .sources-list {
    max-height: 32rem;
    overflow-y: auto;
  }

  .source-row {
    cursor: pointer;
    padding: 0.1rem 0.25rem;

    &:hover {
      background: ${(props) => props.theme.css.tableOddRowColor};
    }

    &.selected {
      background: ${(props) => props.theme.css.primaryLightColor};
    }

    .source-name {
      overflow: hidden;
      text-overflow: ellipsis;
      white-space: nowrap;
    }

    .rule-count {
      text-align: right;
    }

    .default-score .frm-in {
      padding: 0;
    }
  }

  .rules-toolbar h2 {
    flex: 1 1 auto;
  }

  .rules-grid-header,
  .rules-grid-row {
    display: grid;
    grid-template-columns: 2ch 3ch 1.5fr 1fr 1fr 5ch 1.2fr 1fr 5ch 6ch;
    gap: 0.5rem;
    align-items: center;
    padding: 0.25rem;
  }

  .rules-grid-header {
    font-weight: 600;
    border-bottom: 1px solid ${(props) => props.theme.css.lightVariantColor};
  }

  .rules-grid-row {
    background: white;
    border-bottom: 1px solid ${(props) => props.theme.css.tableOddRowColor};

    .score {
      font-weight: 600;
    }
  }

  .drag-handle {
    cursor: grab;

    &.disabled {
      cursor: not-allowed;
      opacity: 0.4;
    }
  }

  .rules-grid-empty {
    padding: 1rem;
    text-align: center;
  }

  .rule-tester,
  .bulk-rescore {
    border-top: 1px solid ${(props) => props.theme.css.lightVariantColor};
    padding-top: 1rem;
    margin-bottom: 2rem;

    .disabled {
      opacity: 0.5;
    }
  }

  .evaluations {
    margin: 0;

    .match {
      font-weight: 600;
    }
  }

  .jobs .error {
    color: ${(props) => props.theme.css.dangerColor};
  }

  .rule-drawer {
    position: fixed;
    top: 0;
    right: 0;
    bottom: 0;
    width: min(32rem, 100vw);
    background: white;
    box-shadow: -0.25rem 0 1rem rgba(0, 0, 0, 0.2);
    z-index: 1000;
    overflow-y: auto;
    padding: 1rem;

    .drawer-actions {
      margin-top: 1rem;
    }
  }
`;
