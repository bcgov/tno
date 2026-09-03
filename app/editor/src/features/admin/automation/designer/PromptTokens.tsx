import React from 'react';

import { CANDIDATE_TOKENS, CONTENT_TOKENS, LOOKUP_TOKENS, TARGET_TOKENS } from './constants';

export interface IPromptTokensProps {
  /** Container holding the target Wysiwyg; tokens insert at the caret when it has focus. */
  editorRef: React.RefObject<HTMLElement | null>;
  /** Append the token to the stored text when the editor does not have focus. */
  onAppend: (token: string) => void;
  /** Show the Candidate group (only meaningful for Detect Duplicate prompts). */
  showCandidates?: boolean;
  /** Show the Target group (only meaningful once the analysis names a target draft). */
  showTarget?: boolean;
}

/**
 * The insertable data-token section shared by the Prompt Library modal and the analysis editor:
 * the token semantics help, then the token groups as click-to-insert buttons. Tokens are replaced
 * at prompt composition, once per item.
 */
export const PromptTokens: React.FC<IPromptTokensProps> = ({
  editorRef,
  onAppend,
  showCandidates = false,
  showTarget = false,
}) => {
  /**
   * Insert a token at the cursor when the prompt editor has focus (mousedown keeps the focus and
   * the caret); otherwise append it to the end of the text.
   */
  const insertToken = (token: string) => {
    const editor = editorRef.current?.querySelector('.ql-editor') as HTMLElement | null;
    if (editor && document.activeElement && editor.contains(document.activeElement)) {
      document.execCommand('insertText', false, token);
      return;
    }
    onAppend(token);
  };

  const group = (label: string, tokens: { token: string; hint: string }[]) => (
    <>
      <label className="automation-token-group-label">{label}</label>
      <div className="automation-token-list">
        {tokens.map(({ token, hint }) => (
          <button
            key={token}
            type="button"
            className="automation-token"
            title={hint}
            onMouseDown={(e) => e.preventDefault()}
            onClick={() => insertToken(token)}
          >
            {token}
          </button>
        ))}
      </div>
    </>
  );

  return (
    <>
      <p className="automation-token-help">
        <strong>Insert Data Token</strong> — the prompt is exactly what is sent to the LLM; tokens
        mark where data is inserted, replaced once per item. <code>{'{content.*}'}</code> is the
        item being processed (its working copy, changes included). In Detect Duplicate prompts{' '}
        <code>{'{candidates}'}</code> inserts the compared stories and{' '}
        <code>{'{candidate.*}'}</code> single fields (iterate mode); the prompt must place them
        itself — see default-dedupe for the layout. <code>{'{lookup:*}'}</code> inserts reference
        lists as JSON (identical for every item). Analyses only: a prompt with no content tokens at
        all gets the story appended as a final '## News Story' section; any{' '}
        <code>{'{content...}'}</code> token disables that. When an analysis names a Target draft,{' '}
        <code>{'{target.*}'}</code> reads that draft instead — including what earlier actions in the
        step have already put on it — while <code>{'{content.*}'}</code> keeps meaning the item the
        iteration started from.
      </p>
      {group('Lookups', LOOKUP_TOKENS)}
      {group('Content (the item being processed)', CONTENT_TOKENS)}
      {showTarget && group('Target (the draft the analysis names)', TARGET_TOKENS)}
      {showCandidates && group('Candidate (Detect Duplicate prompts)', CANDIDATE_TOKENS)}
    </>
  );
};
