import { FormPage } from 'components/formpage';
import styled, { createGlobalStyle } from 'styled-components';

// Imported from the module rather than the 'constants' barrel to keep the component index out of
// this stylesheet's import graph.
import { DATE_PICKER_PORTAL_ID } from '../constants/datePickerPortalId';

export const AutomationProfileForm = styled(FormPage)`
  display: flex;
  flex-direction: column;
  align-items: center;

  /* The form is a centered flex child; without an explicit width it shrinks to its content,
     which makes the tabs resize when switching between tab bodies of different widths. */
  form {
    width: 100%;
  }

  .form-container {
    width: 100%;
  }

  .form-inputs {
    width: 100%;
  }

  .form-top-actions {
    width: 100%;
    margin-bottom: 0.5rem;
    gap: 0.5rem;

    /* Pin the Run button to the left while Save/Delete stay on the right. */
    .run-button {
      margin-right: auto;
    }
  }

  .profile-tabs {
    width: 100%;

    .tab-menu {
      width: 100%;
      /* The design's gold underline replaces tno-core's navy one. */
      border-bottom: solid 3px #fcba19;

      /* The tab bar is a flex Row: the tabs and the Run button sit on the left, the Save/Export/
         Import/Delete action group is pushed to the right. Keep everything vertically centered and
         allow wrapping on narrow screens. */
      /* Bottom-align the row so the tabs sit flush on the gold underline; the action buttons
         lift off it with their own bottom margin. */
      > div {
        align-items: flex-end;
        flex-wrap: wrap;
        row-gap: 0.35rem;
      }

      /* Folder tabs (per the design): inactive tabs are light grey with rounded top corners;
         the active tab is gold and connects to the underline. tno-core's Tab styles its
         background from the 'active' prop (no .active class), so the form stamps .tab-active. */
      .tab {
        background: #f2f4f7;
        border: 1px solid #e4e7ec;
        border-bottom: none;
        border-radius: 0.35rem 0.35rem 0 0;
        padding: 0.4rem 1.2rem;
        margin-bottom: 0;
        font-weight: 600;
        color: #1d2939;

        &.tab-active {
          background: #fcba19;
          border-color: #fcba19;
        }
      }

      .tab-header-actions {
        margin-left: auto;
        display: flex;
        align-items: center;
        gap: 0.5rem;
        /* The action buttons float clear of the gold underline instead of sitting on it. */
        margin-bottom: 0.4rem;
      }

      /* Header action buttons (per the design): green Run, navy-outline secondaries, red-outline
         Delete, solid navy Save. */
      .header-btn-outline {
        background: #fff;
        border: 1px solid #1a3a6b;
        color: #1a3a6b;
        font-weight: 600;

        &:hover:not(:disabled) {
          background: #f5f8ff;
          color: #1a3a6b;
        }
      }

      .header-btn-delete {
        background: #fff;
        border: 1px solid #d8292f;
        color: #d8292f;
        font-weight: 600;

        &:hover:not(:disabled) {
          background: #fff5f5;
          color: #d8292f;
        }
      }

      .header-btn-save {
        background: #1a3a6b;
        border: 1px solid #1a3a6b;
        color: #fff;
        font-weight: 600;

        &:hover:not(:disabled) {
          background: #26428b;
        }
      }

      /* The TabMenu applies 'margin-right: 0.5em' to every non-last-child descendant div. Inside a
         Button that reaches the inner content div, and its ':last-child' status flips as the
         react-tooltip element mounts/unmounts on hover - shifting the button. Neutralize it for our
         buttons; header spacing is handled by the flex gap / run-button margin above. */
      .button > div {
        margin-right: 0;
      }
    }

    /* Center the tab body area within the full-width container. */
    .tab-container {
      width: 100%;
      display: flex;
      flex-direction: column;
      align-items: center;
    }

    /* Stack both tab panels in the same grid cell so the container always has the size of the
       largest panel - switching tabs never resizes or shifts the layout. The inactive panel
       stays rendered but hidden (visibility keeps its footprint, unlike display: none). */
    .tab-panels {
      width: 100%;
      max-width: 100%;
      display: grid;
      grid-template-columns: minmax(0, 100%);
    }

    .tab-panels > .tab-panel {
      grid-area: 1 / 1;
      width: 100%;
      /* Grid items default to min-width auto; without this the runs grid content can widen
         the track beyond the viewport. */
      min-width: 0;
      max-width: 100%;
      overflow-x: hidden;
      visibility: hidden;
      /* Inactive panels stay stacked in the same grid cell. 'visibility: hidden' alone is not
         enough to block clicks: a descendant can re-enable hit-testing with 'visibility: visible'
         (e.g. a react-select control), so a click over the hidden panel could open its dropdown.
         'pointer-events: none' disables interaction for the whole inactive subtree. */
      pointer-events: none;
    }

    .tab-panels > .tab-panel.active {
      visibility: visible;
      pointer-events: auto;
    }
  }

  .runs-tab {
    width: 100%;
    max-width: 100%;
    padding-top: 0.5rem;
    overflow-x: hidden;

    /* Keep the runs grid within the viewport; flex cells must be allowed to shrink
       (min-width: 0) and long values (e.g. run notes) are cut off with an ellipsis. */
    .table,
    .rows,
    .row,
    .header {
      max-width: 100%;
    }

    .row .column,
    .header .column {
      min-width: 0;
      overflow: hidden;
      text-overflow: ellipsis;
      white-space: nowrap;
    }

    /* Flex-item ellipsis requires min-width: 0 on the cell content as well. */
    .row .column > *,
    .row .column .ellipsis {
      min-width: 0;
      max-width: 100%;
      overflow: hidden;
      text-overflow: ellipsis;
      white-space: nowrap;
    }
  }

  .back-button {
    align-self: start;
  }

  .section-header {
    align-items: center;
    justify-content: space-between;
    margin-top: 1rem;

    h2 {
      margin: 0;
      font-size: 1.05rem;
      font-weight: 700;
      letter-spacing: 0.02em;
      text-transform: uppercase;
      color: #1a5a96;
    }
  }

  .section-header-inline {
    justify-content: flex-start;
    gap: 0.35rem;
  }

  .section-header-title {
    align-items: center;
    gap: 0.35rem;
    overflow: visible;
  }

  /* Headings own the row height; without this the h3's default margins push the
     row taller than the text and the info icon renders clipped/misaligned. */
  .section-header-title h3 {
    margin: 0;
    line-height: 1.4;
  }

  .section-help-text {
    margin: 0 0 0.75rem;
    color: #495057;
    font-size: 0.9rem;
    line-height: 1.4;
  }

  .section-doc-button {
    all: unset;
    appearance: none;
    -webkit-appearance: none;
    min-width: 0;
    /* Fixed 1em square, centered on the adjacent text line: the icon can never be
       clipped, and it never changes the row's vertical spacing. */
    width: 1rem;
    height: 1rem;
    font-size: 0.85rem;
    overflow: visible;
    padding: 0;
    margin: 0;
    border: 0 !important;
    border-radius: 0;
    background: transparent !important;
    display: inline-flex;
    align-items: center;
    justify-content: center;
    align-self: center;
    vertical-align: -0.15em;
    line-height: 1;
    flex: 0 0 auto;
    color: #475467;
    box-shadow: none !important;
    cursor: pointer;
  }

  .section-doc-button svg {
    width: 0.85rem;
    height: 0.85rem;
    display: block;
  }

  .section-doc-button::-moz-focus-inner {
    border: 0;
    padding: 0;
  }

  .section-doc-button::before,
  .section-doc-button::after {
    border: 0 !important;
    box-shadow: none !important;
  }

  .section-doc-button:hover {
    background: transparent;
    color: #0f172a;
  }

  .section-doc-button:focus,
  .section-doc-button:focus-visible,
  .section-doc-button:active {
    outline: none;
    outline-offset: 0;
    border: 0 !important;
    box-shadow: none !important;
    background: transparent !important;
  }

  .section-doc-button svg {
    display: block;
  }

  .schedule-field-group {
    display: flex;
    flex-direction: column;
    min-width: 16rem;
    gap: 0.25rem;
  }

  .timezone-field-group {
    min-width: 14rem;
  }

  .field-grid.schedule-row {
    align-items: flex-start;
  }

  .schedule-row > * {
    align-self: flex-start;
  }

  .schedule-header {
    align-items: center;
    justify-content: flex-start;
    gap: 0.35rem;
    min-height: 1.5rem;
  }

  .schedule-label {
    font-size: 0.9rem;
    font-weight: 600;
    color: #212529;
    min-height: 1.5rem;
    display: inline-flex;
    align-items: center;
  }

  .schedule-help-text {
    margin: 0;
    font-size: 0.8rem;
    color: #667085;
  }

  .llm-description {
    margin: 0 0 0.55rem;
    font-size: 0.85rem;
    color: #98a2b3;
    max-width: 40rem;
    align-self: flex-end;
  }

  .filter-row {
    align-items: flex-end;
  }

  .filter-controls {
    display: flex;
    align-items: center;
    gap: 0.5rem;
    margin-left: 0.4rem;
  }

  .step-filter-controls {
    margin-left: 0.25rem;
  }

  .filter-icon-button {
    min-width: auto;
    width: 2.25rem;
    height: 2.25rem;
    padding: 0;
    display: inline-flex;
    align-items: center;
    justify-content: center;
  }

  .field-grid {
    display: flex;
    flex-wrap: wrap;
    align-items: flex-end;
    gap: 0.75rem;
  }

  .description-row {
    width: 100%;
  }

  .description-row .frm-in {
    width: 100%;
    padding-right: 0;
  }

  .step-filter-row {
    align-items: flex-end;
  }

  .step-name-row {
    align-items: flex-start;
  }

  .action-header-row {
    align-items: flex-start;
  }

  .action-main-row {
    align-items: center;
  }

  .schedule-picker {
    position: relative;

    input {
      padding-right: 1.9rem;
    }

    /* Our own icon: react-datepicker's showIcon renders unpredictably inside the SelectDate
       wrapper, so the field owns an absolutely-positioned one instead. */
    .schedule-picker-icon {
      position: absolute;
      right: 0.55rem;
      bottom: 0.5rem;
      pointer-events: none;
      color: #1d2939;
      display: flex;
      align-items: center;

      svg {
        width: 0.95rem;
        height: 0.95rem;
      }
    }

    /* Keep the clearable × from colliding with the icon. */
    .react-datepicker__close-icon {
      right: 1.8rem;
    }
  }

  .modal-intro-text {
    background: #f8f9fa;
    border: 1px solid #d0d5dd;
    border-radius: 0.35rem;
    padding: 0.75rem 1rem;
  }

  .modal-intro-text-legacy {
    margin: 0;
    font-size: 0.9rem;
    line-height: 1.45;
    color: #495057;
  }

  .step-help-text {
    margin: 0.15rem 0 0;
    font-size: 0.85rem;
    line-height: 1.45;
    color: #495057;
  }

  .modal-help-text {
    margin: 0 0 0.1rem;
    font-size: 0.85rem;
    line-height: 1.45;
    color: #98a2b3;
    font-weight: 300;
  }

  .modal-help-text .section-doc-button {
    vertical-align: -0.15em;
    margin-right: 0.2rem;
  }

  .step-prompt-row {
    gap: 0.15rem;
  }

  .action-wysiwyg-row {
    gap: 0.15rem;
  }

  .step-modal-info-icon,
  .step-modal-info-icon:hover,
  .step-modal-info-icon:focus,
  .step-modal-info-icon:focus-visible,
  .step-modal-info-icon:active {
    border: 0 !important;
    outline: 0 !important;
    box-shadow: none !important;
    background: transparent !important;
    display: inline-flex;
    align-items: center;
    justify-content: center;
    width: 1rem;
    height: 1rem;
    font-size: 0.85rem;
    vertical-align: -0.15em;
    overflow: visible;
    color: #475467;
    cursor: pointer;
    line-height: 1;
  }

  .step-modal-info-icon svg {
    width: 0.85rem;
    height: 0.85rem;
    display: block;
  }

  .step-modal-info-icon:hover {
    color: #0f172a;
  }

  .modal-wysiwyg {
    padding-bottom: 0 !important;
    margin-bottom: 0;
  }

  .rules-toolbar {
    margin-bottom: 0.75rem;
  }

  .rules-grid {
    border: 1px solid #dfe3e8;
    border-radius: 0.5rem;
    overflow: hidden;
    margin-bottom: 1rem;
  }

  .schedules-grid {
    border: 1px solid #dfe3e8;
    border-radius: 0.35rem;
    overflow: hidden;
    margin-bottom: 0.75rem;
  }

  .schedules-grid-header,
  .schedules-grid-row {
    display: grid;
    grid-template-columns: minmax(150px, 1fr) 110px minmax(180px, 1fr) 150px 90px 120px;
    gap: 0.5rem;
    align-items: center;
    padding: 0.6rem 0.75rem;
  }

  .schedules-grid-header {
    font-weight: 600;
    background: #f3f6f9;
    border-bottom: 1px solid #dfe3e8;
  }

  .schedules-grid-row {
    border-bottom: 1px solid #eef2f6;
    background: #fff;
  }

  .schedules-grid-row:last-child {
    border-bottom: 0;
  }

  .schedules-grid-row:hover {
    background: #f8fafc;
  }

  .schedules-grid-row .condition-col {
    color: #1a5a96;
  }

  .rules-grid-header,
  .rules-grid-row {
    display: grid;
    grid-template-columns: 40px 28px minmax(150px, 1fr) minmax(220px, 1.3fr) minmax(180px, 1fr) 90px 120px;
    gap: 0.5rem;
    align-items: center;
    padding: 0.6rem 0.75rem;
  }

  .collapse-col {
    display: flex;
    justify-content: center;
  }

  .rules-grid-header {
    font-weight: 600;
    background: #f3f6f9;
    border-bottom: 1px solid #dfe3e8;
  }

  .rules-grid-row {
    border-bottom: 1px solid #eef2f6;
    background: #fff;
  }

  .rules-grid-row:last-child {
    border-bottom: 0;
  }

  .rules-grid-row:hover {
    background: #f8fafc;
  }

  .rules-grid-body,
  .actions-sub-grid-body {
    display: flex;
    flex-direction: column;
  }

  .step-row-container {
    border-bottom: 1px solid #eef2f6;
    transition: border-color 0.15s ease, box-shadow 0.15s ease;
  }

  .step-row-container:last-child {
    border-bottom: 0;
  }

  .actions-sub-grid {
    margin: 0 0.75rem 0.75rem 3rem;
    border: 1px solid #e4e7ec;
    border-radius: 0.4rem;
    overflow: hidden;
    background: #fcfcfd;
  }

  .actions-sub-grid-header,
  .actions-sub-grid-row {
    display: grid;
    grid-template-columns: 32px minmax(150px, 1fr) minmax(220px, 1.5fr) 90px 90px 72px;
    gap: 0.5rem;
    align-items: center;
    padding: 0.45rem 0.6rem;
  }

  .actions-sub-grid-header {
    background: #f8fafc;
    font-weight: 600;
    border-bottom: 1px solid #e4e7ec;
  }

  .actions-sub-grid-row {
    border-bottom: 1px solid #eef2f6;
  }

  .actions-sub-grid-row:last-child {
    border-bottom: 0;
  }

  .step-row-container.is-dragging,
  .actions-sub-grid-row.is-dragging {
    background: #eff6ff;
    box-shadow: 0 2px 8px rgba(16, 24, 40, 0.12);
    border-radius: 0.3rem;
  }

  .rules-grid-empty {
    padding: 1rem;
    color: #6c757d;
  }

  .drag-col {
    display: flex;
    justify-content: center;
    cursor: grab;
    color: #64748b;
  }

  .actions-col {
    display: flex;
    flex-direction: row;
    flex-wrap: nowrap;
    align-items: center;
    justify-content: flex-end;
    justify-self: end;
    width: 100%;
    gap: 0.6rem;
    white-space: nowrap;
    text-align: right;
  }

  .rules-grid-header .actions-col,
  .rules-grid-row .actions-col,
  .actions-sub-grid-header .actions-col,
  .actions-sub-grid-row .actions-col {
    margin-left: auto;
    justify-content: flex-end;
  }

  .rule-icon-button {
    border: 0;
    border-radius: 0;
    background: transparent;
    padding: 0;
    display: inline-flex;
    align-items: center;
    justify-content: center;
    cursor: pointer;
    color: #475467;
    font-size: 1rem;
    line-height: 1;
  }

  .rule-icon-button:hover:not(:disabled) {
    color: #0f172a;
  }

  .rule-icon-button.delete:hover:not(:disabled) {
    color: #b42318;
  }

  .rule-icon-button:disabled {
    opacity: 0.45;
    cursor: not-allowed;
  }

  .step-collapse-toggle {
    font-size: 0.75rem;
  }

  .step-action-count {
    margin-left: 0.5rem;
    color: #667085;
    font-size: 0.85rem;
  }

  .rule-modal-content {
    display: flex;
    flex-direction: column;
    gap: 0.5rem;
    min-width: min(60rem, 90vw);
  }

  .section-doc-content {
    p {
      margin-top: 0;
      margin-bottom: 0.75rem;
      line-height: 1.4;
    }

    ul {
      margin: 0;
      padding-left: 1.25rem;
      line-height: 1.5;
    }
  }

  @media (max-width: 1200px) {
    .rules-grid {
      overflow-x: auto;
    }

    .rules-grid-header,
    .rules-grid-row {
      min-width: 860px;
    }

    .actions-sub-grid-header,
    .actions-sub-grid-row {
      min-width: 760px;
    }
  }

  /* Debugging tab: chat history. */
  .debug-selected {
    font-size: 0.9rem;
    padding: 0.25rem 0.5rem;
    background: #eef3fb;
    border-radius: 4px;
  }

  .debug-results {
    list-style: none;
    margin: 0;
    padding: 0;
    max-height: 180px;
    overflow-y: auto;
    border: 1px solid #e0e0e0;
    border-radius: 4px;

    .debug-result {
      display: block;
      width: 100%;
      text-align: left;
      background: none;
      border: none;
      border-bottom: 1px solid #eee;
      padding: 0.4rem 0.6rem;
      cursor: pointer;

      &:hover {
        background: #f2f6fc;
      }
      &.selected {
        background: #dbe8ff;
      }
      .id {
        font-weight: 600;
        margin-right: 0.25rem;
      }
      .source {
        color: #666;
      }
    }
  }

  /* Scrollable conversation; user prompts right-aligned, LLM responses left-aligned. */
  .debug-transcript {
    /* tno-core's Col only sets flex-direction:column when given a direction prop; without it the
       flex default is row. It also sets flex-wrap:wrap, so a column with a constrained max-height
       wraps overflowing messages into a new column to the RIGHT (the bug) instead of scrolling.
       Force a real, non-wrapping column so responses stack, align left/right, and scroll. */
    display: flex;
    flex-direction: column;
    flex-wrap: nowrap;
    align-items: stretch;
    width: 100%;
    max-height: 420px;
    overflow-y: auto;
    padding: 0.75rem;
    border: 1px solid #dcdcdc;
    border-radius: 6px;
    background: #f7f8fa;

    .debug-message {
      max-width: 85%;
      padding: 0.5rem 0.75rem;
      border-radius: 10px;

      .role {
        font-size: 0.7rem;
        font-weight: 700;
        text-transform: uppercase;
        letter-spacing: 0.03em;
        opacity: 0.6;
        margin-bottom: 0.2rem;
      }
      .text {
        font-family: inherit;
        font-size: 0.9rem;
      }
      /* Long messages are collapsed to a few lines by default; the toggle expands them. */
      .text.collapsed {
        display: -webkit-box;
        -webkit-line-clamp: 8;
        -webkit-box-orient: vertical;
        overflow: hidden;
      }
      .debug-toggle {
        margin-top: 0.25rem;
        padding: 0;
        background: none;
        border: none;
        color: #1a5a96;
        font-size: 0.75rem;
        cursor: pointer;
        text-decoration: underline;
      }

      /* LLM responses: left. */
      &.assistant {
        align-self: flex-start;
        background: #ffffff;
        border: 1px solid #e2e2e2;
        border-bottom-left-radius: 2px;
      }

      /* User prompts: right. */
      &.user {
        align-self: flex-end;
        background: #d6e4ff;
        border-bottom-right-radius: 2px;
        .role {
          text-align: right;
        }
      }
    }
  }

  /* ---- v2 designer, log viewer, and run outcome ---- */
  .v2-designer,
  .v2-step-editor,
  .v2-action-editor,
  .v2-analysis-editor {
    width: 100%;
  }

  .v2-step-card {
    border: 1px solid #d0d5dd;
    border-radius: 0.35rem;
    padding: 0.5rem;
    background: #fff;
  }

  .v2-step-card-header {
    cursor: default;

    strong {
      flex: 0 1 auto;
    }
  }

  .v2-list-item {
    border: 1px solid #e4e7ec;
    border-radius: 0.35rem;
    padding: 0.4rem;
    background: #fcfcfd;
    width: 100%;
  }

  .v2-steps-list .v2-step-card {
    margin-bottom: 0.5rem;
  }

  .v2-actions-list .v2-list-item {
    margin-bottom: 0.4rem;
  }

  .v2-step-card.is-dragging,
  .v2-list-item.is-dragging {
    box-shadow: 0 4px 12px rgba(16, 24, 40, 0.15);
  }

  .v2-drag-handle {
    display: flex;
    align-items: center;
    padding: 0.35rem 0.2rem;
    color: #98a2b3;
    cursor: grab;

    &:hover {
      color: #475467;
    }
  }

  /* Action fields flow side by side and wrap; structured editors take their own line. */
  .v2-action-fields {
    row-gap: 0.5rem;
  }

  .v2-field-wide {
    flex-basis: 100%;
  }

  /* Steps grid: header row, draggable rows, expanded analyses/actions subgrids. */
  .v2-grid {
    border: 1px solid #dfe3e8;
    border-radius: 0.35rem;
    background: #fff;
    width: 100%;
  }

  .v2-grid-header,
  .v2-grid-row {
    display: flex;
    align-items: center;
    gap: 0.5rem;
    padding: 0.45rem 0.6rem;
    width: 100%;
  }

  .v2-grid-header {
    font-weight: 600;
    background: #f2f4f7;
    border-bottom: 1px solid #dfe3e8;
  }

  .v2-grid-item {
    border-bottom: 1px solid #eef2f6;
    background: #fff;

    /* Zebra rows per the design (the droppable wraps the rows; header sits outside it). */
    &:nth-child(even) {
      background: #f7f8fa;
    }

    &:last-child {
      border-bottom: 0;
    }

    &.is-dragging {
      box-shadow: 0 4px 12px rgba(16, 24, 40, 0.15);
    }
  }

  .v2-grid-row:hover {
    background: #f8fafc;
  }

  .v2-gc-drag {
    width: 1.5rem;
    flex: 0 0 auto;
  }

  .v2-gc-collapse {
    width: 1.75rem;
    flex: 0 0 auto;
  }

  .v2-gc-name {
    flex: 2 1 0;
    min-width: 0;
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
  }

  .v2-gc-phase {
    width: 7rem;
    flex: 0 0 auto;
  }

  .v2-gc-source {
    flex: 1.5 1 0;
    min-width: 0;
  }

  .v2-gc-clip {
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
  }

  .v2-gc-count {
    width: 5rem;
    flex: 0 0 auto;
  }

  .v2-gc-save {
    width: 7rem;
    flex: 0 0 auto;
  }

  .v2-gc-enabled {
    width: 5rem;
    flex: 0 0 auto;
  }

  .v2-gc-actions {
    width: 7rem;
    flex: 0 0 auto;
    display: flex;
    justify-content: flex-end;
    gap: 0.6rem;

    .rule-icon-button,
    .rule-icon-button.delete {
      color: #1a5a96;

      &:hover:not(:disabled) {
        color: #0f3e6d;
      }
    }
  }

  .v2-grid-expanded {
    padding: 0.5rem 0.75rem 0.75rem 3.4rem;
    border-top: 1px dashed #e4e7ec;
    background: #fcfcfd;
  }

  .v2-subgrid {
    margin-bottom: 0.5rem;
    font-size: 0.9rem;

    .v2-grid-header,
    .v2-grid-row {
      padding: 0.3rem 0.5rem;
    }
  }

  /* Prompt library table. */
  .v2-library-table {
    width: 100%;
    border-collapse: collapse;
    font-size: 0.9rem;
    border: 1px solid #e4e7ec;
    border-radius: 0.35rem;

    th,
    td {
      border-bottom: 1px solid #e4e7ec;
      padding: 0.4rem 0.6rem;
      text-align: left;
      vertical-align: middle;
    }

    th {
      background: #f2f4f7;
      font-weight: 600;
    }
  }

  .v2-col-name {
    width: 14rem;
  }

  .v2-col-refs {
    width: 14rem;
  }

  .v2-col-actions {
    width: 5.5rem;
    text-align: right !important;
    white-space: nowrap;

    .rule-icon-button + .rule-icon-button {
      margin-left: 0.6rem;
    }

    .rule-icon-button,
    .rule-icon-button.delete {
      color: #1a5a96;

      &:hover:not(:disabled) {
        color: #0f3e6d;
      }
    }
  }

  .v2-library-empty {
    color: #667085;
    text-align: center;
  }

  .v2-prompt-name {
    font-family: monospace;
    font-size: 0.8rem;
    background: #f2f4f7;
    border-radius: 0.25rem;
    padding: 0.1rem 0.4rem;
  }

  .v2-chip-open {
    border: none;
    background: none;
    padding: 0;
    cursor: pointer;
    color: #1570ef;
    font-size: 0.8rem;

    &:hover {
      text-decoration: underline;
    }
  }

  .v2-subsection-header {
    align-items: center;
    gap: 0.5rem;

    h3 {
      margin: 0;
    }
  }

  .v2-field-help {
    margin: 0;
    font-size: 0.8rem;
    color: #667085;
  }

  .section-help-text .help-accent {
    color: #2b7a78;
  }

  .v2-badge {
    display: inline-block;
    padding: 0.05rem 0.5rem;
    border-radius: 0.75rem;
    background: #eaecf0;
    color: #344054;
    font-size: 0.75rem;
    white-space: nowrap;
  }

  .v2-badge-success {
    background: #d1fadf;
    color: #05603a;
  }

  .v2-badge-warning {
    background: #fef0c7;
    color: #93370d;
  }

  .v2-badge-danger {
    background: #fee4e2;
    color: #912018;
  }

  .v2-phase-init {
    background: #e0eaff;
    color: #26428b;
  }

  .v2-phase-process {
    background: #d1fadf;
    color: #05603a;
  }

  .v2-phase-complete {
    background: #fce7f6;
    color: #9e165f;
  }

  .v2-chips {
    flex-wrap: wrap;
  }

  .v2-chip {
    display: inline-flex;
    align-items: center;
    gap: 0.25rem;
    padding: 0.1rem 0.5rem;
    border-radius: 0.75rem;
    background: #eaecf0;
    font-size: 0.8rem;

    button {
      border: none;
      background: none;
      cursor: pointer;
      color: #667085;
      padding: 0;
    }
  }

  /* Filter fields per the design: pencil inside the control (left of the ×/▼ indicators),
     compact + attached at the right. */
  .v2-filter-field {
    .v2-filter-select-wrap {
      position: relative;
      display: inline-block;

      /* Keep the selected label clear of the overlaid pencil. */
      .rs__value-container {
        padding-right: 1.7rem;
      }
    }

    .v2-filter-edit {
      position: absolute;
      right: 3.9rem;
      bottom: 0.55rem;
      border: none;
      background: none;
      padding: 0;
      cursor: pointer;
      color: #1a5a96;
      display: flex;
      align-items: center;

      svg {
        width: 0.95rem;
        height: 0.95rem;
      }

      &:hover {
        color: #0f3e6d;
      }
    }

    .v2-filter-add {
      width: 2.1rem;
      height: 2.35rem;
      margin: 0 0 0.05rem 0.3rem;
      display: inline-flex;
      align-items: center;
      justify-content: center;
      border: 1px solid #1a5a96;
      border-radius: 0.25rem;
      background: #fff;
      color: #1a5a96;
      cursor: pointer;

      &:hover {
        background: #f5f8ff;
      }
    }
  }

  .v2-scoped-name label {
    display: block;
  }

  .v2-scoped-name-input {
    gap: 0.15rem;
  }

  .v2-scope-prefix {
    font-family: monospace;
    color: #475467;
    background: #f2f4f7;
    border: 1px solid #d0d5dd;
    border-right: none;
    border-radius: 0.25rem 0 0 0.25rem;
    padding: 0.3rem 0.35rem;
  }

  .v2-link-button {
    border: none;
    background: none;
    color: #1570ef;
    cursor: pointer;
    padding: 0;
    font-size: 0.85rem;
    text-align: left;
  }

  .v2-condition-children {
    margin-left: 1.5rem;
    padding-left: 0.5rem;
    border-left: 2px solid #e4e7ec;
  }

  .v2-findings {
    border: 1px solid #fda29b;
    border-radius: 0.35rem;
    padding: 0.5rem;
    background: #fffbfa;

    code {
      font-size: 0.8rem;
    }
  }
`;

/**
 * Constrains the step/action modals to the browser viewport: the popup never grows taller than
 * the viewing area, the body scrolls, and the footer buttons remain visible. Applied as a global
 * style (mounted by the form) because the tno-core Modal renders through a React portal
 * (document.body); scoped to these modals via their '.rule-modal-content' body root.
 */
export const AutomationModalStyles = createGlobalStyle`
  .header-btn-outline {
    background: #fff;
    border: 1px solid #1a3a6b;
    color: #1a3a6b;
    font-weight: 600;

    &:hover:not(:disabled) {
      background: #f5f8ff;
      color: #1a3a6b;
    }
  }

  .header-btn-delete {
    background: #fff;
    border: 1px solid #d8292f;
    color: #d8292f;
    font-weight: 600;

    &:hover:not(:disabled) {
      background: #fff5f5;
      color: #d8292f;
    }
  }

  .header-btn-save {
    background: #1a3a6b;
    border: 1px solid #1a3a6b;
    color: #fff;
    font-weight: 600;

    &:hover:not(:disabled) {
      background: #26428b;
    }
  }

  .modal-popup:has(.rule-modal-content) {
    max-height: calc(100vh - 3rem);
    overflow: hidden;
    /* Size to the form instead of filling the screen. */
    width: auto;
    min-width: 36rem;
    max-width: min(52rem, 93vw);
  }

  .modal-popup:has(.rule-modal-content) .modal-body {
    flex: 1 1 auto;
    min-height: 0;
    overflow-y: auto;
    padding-right: 0.25rem;
  }

  .modal-popup:has(.rule-modal-content) .button-row {
    flex-shrink: 0;
  }

  /* The schedule modal clips its popup ('overflow: hidden' above) and scrolls its body, so the
     Start After calendar is rendered into a body-level portal ('portalId') instead of inline.
     Both the modal and the portal are children of document.body, so the popper has to out-rank
     the modal wrapper's z-index (1050) to paint over the dialog. */
  #${DATE_PICKER_PORTAL_ID} .react-datepicker-popper {
    z-index: 1060;
  }

  /* Full-width select rows (Prior Action, Report): the whole label is always visible.
     Lives here (not in the form's styled root) because the modal renders through a portal.
     Value/option wrapping is handled by the react-select styles prop on the field. */
  .rule-modal-content .action-wide-select-row > .frm-in {
    flex: 1 1 100%;
    max-width: 100%;
  }

  .rule-modal-content .action-wide-select-row .frm-in > div {
    width: 100%;
  }

  /* Run detail modal content. */
  .v2-prompt-modal .v2-token-help {
    margin: 0.5rem 0 0.25rem 0;
    font-size: 0.85rem;
    color: #475467;
  }

  .v2-prompt-modal .v2-token-group-label {
    font-size: 0.75rem;
    font-weight: 700;
    letter-spacing: 0.03em;
    text-transform: uppercase;
    color: #667085;
  }

  .v2-prompt-modal .v2-token-list {
    display: flex;
    flex-wrap: wrap;
    gap: 0.35rem;
    margin-bottom: 0.5rem;
  }

  .v2-prompt-modal .v2-token {
    font-family: monospace;
    font-size: 0.75rem;
    color: #26428b;
    background: #f5f8ff;
    border: 1px solid #b2ccff;
    border-radius: 0.9rem;
    padding: 0.15rem 0.5rem;
    cursor: pointer;

    &:hover {
      background: #e0eaff;
    }
  }

  .run-detail-content .v2-run-detail-toggle {
    margin: 0.5rem 0;
  }

  .run-detail-content .v2-dry-run-banner {
    padding: 0.4rem 0.75rem;
    border-radius: 0.35rem;
    background: #fef0c7;
    color: #93370d;
    font-weight: 600;
  }

  .run-detail-content .v2-table {
    width: 100%;
    border-collapse: collapse;
    font-size: 0.85rem;

    th,
    td {
      border: 1px solid #e4e7ec;
      padding: 0.25rem 0.5rem;
      text-align: left;
      vertical-align: top;
    }

    th {
      background: #f9fafb;
    }
  }

  .run-detail-content .v2-cell-clip {
    max-width: 20rem;
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
  }

  .run-detail-content .v2-log-entry {
    border: 1px solid #e4e7ec;
    border-radius: 0.35rem;
    padding: 0.35rem 0.5rem;
    background: #fff;
  }

  .run-detail-content .v2-log-entry-decision {
    background: #f9fafb;
  }

  .run-detail-content .v2-log-entry-header {
    cursor: pointer;
    flex-wrap: wrap;
  }

  .run-detail-content .v2-log-entry-body pre {
    max-height: 20rem;
    overflow: auto;
    white-space: pre-wrap;
    background: #f2f4f7;
    border-radius: 0.25rem;
    padding: 0.5rem;
    margin: 0.25rem 0;
  }

  .run-detail-content .v2-badge {
    display: inline-block;
    padding: 0.05rem 0.5rem;
    border-radius: 0.75rem;
    background: #eaecf0;
    color: #344054;
    font-size: 0.75rem;
    white-space: nowrap;
  }

  .run-detail-content .v2-badge-success {
    background: #d1fadf;
    color: #05603a;
  }

  .run-detail-content .v2-badge-warning {
    background: #fef0c7;
    color: #93370d;
  }

  .run-detail-content .v2-badge-danger {
    background: #fee4e2;
    color: #912018;
  }

  .run-detail-content .v2-field-help {
    margin: 0;
    font-size: 0.8rem;
    color: #667085;
  }

  .run-detail-content .v2-explain-panel {
    border-top: 1px solid #e4e7ec;
    padding-top: 0.5rem;
  }

  .run-detail-content .v2-explain-exchange {
    border: 1px solid #e4e7ec;
    border-radius: 0.35rem;
    padding: 0.5rem;
  }

  .run-detail-content .v2-explain-suggestion {
    border: 1px dashed #84caff;
    border-radius: 0.35rem;
    padding: 0.5rem;
    background: #f5faff;
  }

  .run-detail-content .run-detail-summary {
    display: grid;
    grid-template-columns: repeat(2, minmax(14rem, 1fr));
    gap: 0.25rem 1rem;
    margin-bottom: 0.5rem;
  }

  .run-detail-content label {
    font-weight: 600;
  }

  .run-detail-content .run-detail-note {
    margin-bottom: 0.5rem;
  }

  .run-detail-content h3 {
    margin: 0.5rem 0 0.25rem;
  }

  .run-detail-content .run-detail-outcomes {
    background: #f8fafc;
    border: 1px solid #e4e7ec;
    border-radius: 0.3rem;
    padding: 0.5rem;
    font-size: 0.85rem;
    overflow-x: auto;
  }

  .run-detail-content .run-detail-responses {
    display: flex;
    flex-direction: column;
    gap: 0.5rem;
    max-height: 20rem;
    overflow-y: auto;
  }

  .run-detail-content .run-detail-prompt {
    margin: 0.25rem 0;

    summary {
      cursor: pointer;
      font-size: 0.8rem;
      color: #667085;
    }

    pre {
      margin: 0.25rem 0 0;
      white-space: pre-wrap;
      word-break: break-word;
      max-height: 15rem;
      overflow-y: auto;
      background: #eef2f6;
      border-radius: 0.25rem;
      padding: 0.35rem;
    }
  }

  .run-detail-content .run-detail-response {
    background: #f8fafc;
    border: 1px solid #e4e7ec;
    border-radius: 0.3rem;
    padding: 0.5rem;
    font-size: 0.85rem;

    label {
      font-weight: 600;
    }

    pre {
      margin: 0.25rem 0 0;
      white-space: pre-wrap;
      word-break: break-word;
    }
  }

  /* Quill styles paragraphs at 1rem (via tno-core) but leaves list items at its 13px container
     default, and indents lists twice (ol + li padding). Match list text to paragraph text and
     reduce the indent. */
  .modal-popup:has(.rule-modal-content) .ql-editor li {
    font-family: ${(props) => props.theme.css?.bcSans};
    font-size: 1rem;
    padding-left: 1.1em;
    margin: 0.15rem 0;
  }

  .modal-popup:has(.rule-modal-content) .ql-editor ol,
  .modal-popup:has(.rule-modal-content) .ql-editor ul {
    padding-left: 0.5em;
  }

  .modal-popup:has(.rule-modal-content) .ql-editor li > .ql-ui:before {
    margin-left: -1.1em;
    margin-right: 0.3em;
    width: 0.8em;
  }
`;

/**
 * Wraps the Enabled checkbox in the step/action modals so it vertically centers against the
 * adjacent input control. Defined as its own styled-component because the tno-core Modal renders
 * through a React portal (document.body), so the form wrapper's classes do not reach the modal.
 * The top padding offsets the adjacent field's label height.
 */
export const ModalEnabledCheckbox = styled.div`
  min-height: 2.25rem;
  display: inline-flex;
  align-items: center;
  padding-top: 1.45rem;

  /* When the checkbox sits on its own line (no adjacent labelled input) the label offset is
     unnecessary space. */
  &.no-label-offset {
    padding-top: 0;
    min-height: 0;
  }
`;

/**
 * Wraps a modal WYSIWYG field and its help text. Defined as its own styled-component (not a class
 * on the form's styled wrapper) because the tno-core Modal renders through a React portal
 * (document.body), so the form wrapper's classes do not reach the modal. This removes the WYSIWYG's
 * default bottom padding and the help paragraph's default margins so the help text sits directly
 * beneath the editor.
 */
export const ModalPromptField = styled.div`
  position: relative;
  display: flex;
  flex-direction: column;
  width: 100%;

  .modal-wysiwyg {
    padding-bottom: 0;
    margin-bottom: 0;
  }

  .modal-help-text {
    margin: 0.1rem 0 0;
    font-size: 0.85rem;
    line-height: 1.45;
    color: #98a2b3;
    font-weight: 300;
  }

  .step-modal-info-icon {
    display: inline-flex;
    align-items: center;
    color: #475467;
    cursor: pointer;
  }
`;

/**
 * Wraps the step modal "Target" select so its help icon can be overlaid next to the label.
 * Defined as its own styled-component (not a class on the form's styled wrapper) because the
 * tno-core Modal renders through a React portal (document.body), so descendant-scoped classes
 * from the form wrapper do not apply to the modal contents.
 */
export const StepTargetWithHelp = styled.div`
  position: relative;
`;

/**
 * The step Prompt "reset to default" icon button, overlaid immediately after the "Prompt" label
 * rendered by the Wysiwyg component. Positioned absolutely (within ModalPromptField) so it does
 * not affect the editor layout.
 */
export const PromptResetButton = styled.button`
  position: absolute;
  top: 0.2rem;
  left: 4.35rem;
  width: 1rem;
  height: 1rem;
  font-size: 0.85rem;
  overflow: visible;
  padding: 0;
  margin: 0;
  border: 0;
  background: transparent;
  box-shadow: none;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  line-height: 1;
  color: #475467;
  cursor: pointer;

  svg {
    width: 0.85rem;
    height: 0.85rem;
    display: block;
  }

  &:hover {
    color: #0f172a;
  }

  &:focus,
  &:focus-visible,
  &:active {
    outline: none;
    box-shadow: none;
  }
`;

/**
 * The action modal "Action Prompt" label help icon, overlaid immediately after the label
 * rendered by the Wysiwyg component (within ModalPromptField).
 */
export const ActionPromptHelpButton = styled(PromptResetButton)`
  left: 7.75rem;
`;

/**
 * The "Target" label help icon. Rendered plain (no border/background) to match the other
 * info icons, and absolutely positioned immediately after the "Target" label text so it does
 * not affect the select's layout or vertical alignment.
 */
export const StepTargetHelpButton = styled.button`
  position: absolute;
  top: 0.2rem;
  left: 3.55rem;
  width: 1rem;
  height: 1rem;
  font-size: 0.85rem;
  overflow: visible;
  padding: 0;
  margin: 0;
  border: 0;
  background: transparent;
  box-shadow: none;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  line-height: 1;
  color: #475467;
  cursor: pointer;

  svg {
    width: 0.85rem;
    height: 0.85rem;
    display: block;
  }

  &:hover {
    color: #0f172a;
  }

  &:focus,
  &:focus-visible,
  &:active {
    outline: none;
    box-shadow: none;
  }
`;
