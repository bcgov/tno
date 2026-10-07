import styled from 'styled-components';

export const TopicScoreAdmin = styled.div`
  width: 100%;
  flex: 1 1 0;
  min-height: 0;
  display: flex;
  overflow: hidden;

  > .form-page {
    display: flex;
    flex-direction: column;
    min-width: 0;
    min-height: 0;
    padding-bottom: 1rem;
  }

  .topic-score-tabs {
    flex: 1 1 0;
    min-height: 0;

    > .tab-container {
      display: flex;
      flex-direction: column;
      min-height: 0;
      height: auto;
      padding-top: 1rem;
    }
  }

  .tab-panel {
    flex: 1 1 0;
    min-height: 0;
    overflow: auto;
  }

  .scoring-tab:not([hidden]) {
    display: flex;
    overflow: hidden;
  }

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
    grid-template-columns: minmax(18rem, 1fr) minmax(0, 3fr);
    gap: 1rem;
    flex: 1 1 0;
    min-width: 0;
    min-height: 0;
  }

  .sources-pane,
  .rules-pane {
    display: flex;
    flex-direction: column;
    min-width: 0;
    min-height: 0;
    padding: 1rem;
    border: 1px solid ${(props) => props.theme.css.lightVariantColor};
    border-radius: 0.35rem;
    background: white;

    > :not(.sources-list):not(.rules-grid) {
      flex-shrink: 0;
    }
  }

  .sources-pane {
    display: flex;
    flex-direction: column;
    gap: 0.25rem;
  }

  .sources-header,
  .source-row {
    display: grid;
    grid-template-columns: minmax(0, 1fr) 12ch 2rem;
    gap: 0.5rem;
    align-items: center;
  }

  .sources-header {
    font-weight: 600;
    border-bottom: 1px solid ${(props) => props.theme.css.lightVariantColor};
  }

  .default-score-header,
  .default-score input {
    text-align: right;
  }

  .default-score {
    display: flex;
    justify-content: flex-end;
  }

  .sources-list,
  .rules-grid {
    flex: 1 1 0;
    min-height: 0;
    overflow: auto;
    overscroll-behavior: contain;
    scrollbar-gutter: stable;
  }

  .rules-grid-header,
  .rules-grid-row {
    min-width: 48rem;
  }

  .rules-grid-header {
    position: sticky;
    top: 0;
    z-index: 1;
    background: white;
  }

  .source-row {
    cursor: pointer;
    padding: 0.1rem 0.25rem;

    &:nth-child(even) {
      background: ${(props) => props.theme.css.tableEvenRowColor};
    }

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

    .default-score .frm-in {
      padding: 0;
    }
  }

  .remove-source {
    color: ${(props) => props.theme.css.dangerColor};
    padding: 0.25rem;
  }

  .rules-toolbar .frm-in,
  .rule-tester .frm-in,
  .bulk-rescore .frm-in {
    padding-bottom: 0;
  }

  .sources-toolbar h2,
  .rules-toolbar h2 {
    flex: 1 1 auto;
  }

  .rules-grid-header,
  .rules-grid-row {
    display: grid;
    grid-template-columns: 2ch 3ch 1.5fr 1fr 1fr 5ch 1.2fr 1fr 5ch 7ch;
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

    &:nth-child(even) {
      background: ${(props) => props.theme.css.tableEvenRowColor};
    }

    .score {
      font-weight: 600;
    }
  }

  .rule-actions {
    display: flex;
    gap: 0.25rem;

    button {
      padding: 0.25rem;
    }

    .delete-rule {
      color: ${(props) => props.theme.css.dangerColor};
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
    padding: 0 0.5rem 1rem;

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

  .section-field .frm-select {
    margin-right: 0.5em;
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
