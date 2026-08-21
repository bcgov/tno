import React from 'react';

import { V2_CANDIDATE_TOKENS, V2_CONTENT_TOKENS, V2_LOOKUP_TOKENS } from './constants';

export interface IV2PromptTokensProps {
  /** Container holding the target Wysiwyg; tokens insert at the caret when it has focus. */
  editorRef: React.RefObject<HTMLElement | null>;
  /** Append the token to the stored text when the editor does not have focus. */
  onAppend: (token: string) => void;
  /** Show the Candidate group (only meaningful for Detect Duplicate prompts). */
  showCandidates?: boolean;
}

/**
 * The insertable data-token section shared by the Prompt Library modal and the analysis editor:
 * the token semantics help, then the token groups as click-to-insert buttons. Tokens are replaced
 * at prompt composition, once per item.
 */
export const V2PromptTokens: React.FC<IV2PromptTokensProps> = ({
  editorRef,
  onAppend,
  showCandidates = false,
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
      <label className="v2-token-group-label">{label}</label>
      <div className="v2-token-list">
        {tokens.map(({ token, hint }) => (
          <button
            key={token}
            type="button"
            className="v2-token"
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
      <p className="v2-token-help">
        <strong>Insert Data Token</strong> — the prompt is exactly what is sent to the LLM; tokens
        mark where data is inserted, replaced once per item. <code>{'{content.*}'}</code> is the
        item being processed (its working copy, changes included). In Detect Duplicate prompts{' '}
        <code>{'{candidates}'}</code> inserts the compared stories and{' '}
        <code>{'{candidate.*}'}</code> single fields (iterate mode); the prompt must place them
        itself — see default-dedupe for the layout. <code>{'{lookup:*}'}</code> inserts reference
        lists (identical for every item). Analyses only: a prompt with no content tokens at all gets
        the story appended as a final '## News Story' section; any <code>{'{content...}'}</code>{' '}
        token disables that.
      </p>
      {group('Lookups', V2_LOOKUP_TOKENS)}
      {group('Content (the item being processed)', V2_CONTENT_TOKENS)}
      {showCandidates && group('Candidate (Detect Duplicate prompts)', V2_CANDIDATE_TOKENS)}
    </>
  );
};
