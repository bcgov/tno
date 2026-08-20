import React from 'react';
import { FaCopy, FaEdit, FaPlus, FaTrash } from 'react-icons/fa';
import { toast } from 'react-toastify';
import { Button, ButtonVariant, Col, Modal, Row, Show, Text, TextArea, Wysiwyg } from 'tno-core';

import {
  V2_CANDIDATE_TOKENS,
  V2_CONTENT_TOKENS,
  V2_DEFAULT_PROMPTS,
  V2_LOOKUP_TOKENS,
} from './constants';
import { type IV2Definition, type IV2Prompt } from './interfaces';

export interface IV2PromptLibraryProps {
  definition: IV2Definition;
  onChange: (definition: IV2Definition) => void;
}

interface IPromptDraft {
  /** The entry being edited, or null when adding. */
  originalName: string | null;
  name: string;
  description: string;
  text: string;
}

/** Where a definition references a prompt entry (analyses and dedupe actions carry prompts). */
const promptRefs = (
  definition: IV2Definition,
): { name: string; step: string; override: boolean }[] => {
  const refs: { name: string; step: string; override: boolean }[] = [];
  const add = (prompt: IV2Prompt | null | undefined, step: string) => {
    if (prompt?.ref) refs.push({ name: prompt.ref, step, override: !!prompt.override?.trim() });
  };
  definition.steps.forEach((step) => {
    step.analyses.forEach((analysis) => add(analysis.prompt, step.name));
    step.actions.forEach((action) => add(action.prompt, step.name));
  });
  return refs;
};

/** Rewrite every reference from one prompt name to another (rename support). */
const renameRefs = (definition: IV2Definition, from: string, to: string): IV2Definition => ({
  ...definition,
  steps: definition.steps.map((step) => ({
    ...step,
    analyses: step.analyses.map((analysis) =>
      analysis.prompt?.ref === from
        ? { ...analysis, prompt: { ...analysis.prompt, ref: to } }
        : analysis,
    ),
    actions: step.actions.map((action) =>
      action.prompt?.ref === from ? { ...action, prompt: { ...action.prompt, ref: to } } : action,
    ),
  })),
});

/**
 * The prompt library: shared prompt text stored once and referenced by steps and actions (a step
 * stores only its override). Rendered as a table - name, description, where it is referenced -
 * with an add/edit modal offering insertable data tokens that are replaced at prompt composition.
 */
export const V2PromptLibrary: React.FC<IV2PromptLibraryProps> = ({ definition, onChange }) => {
  const [draft, setDraft] = React.useState<IPromptDraft | null>(null);
  const editorRef = React.useRef<HTMLDivElement>(null);

  const names = Object.keys(definition.prompts);
  const refs = promptRefs(definition);
  // Engine built-ins render as editable placeholder rows, but only when the definition
  // actually uses the action that sends them.
  const missingDefaults = Object.keys(V2_DEFAULT_PROMPTS).filter(
    (name) =>
      !names.includes(name) &&
      definition.steps.some((step) =>
        step.actions.some((action) => action.type === V2_DEFAULT_PROMPTS[name].action),
      ),
  );

  const referencedBy = (name: string): string => {
    if (name in V2_DEFAULT_PROMPTS) return 'built-in default';
    const mine = refs.filter((ref) => ref.name === name);
    const steps = new Set(mine.map((ref) => ref.step));
    if (steps.size === 0) return 'unused';
    const overrides = new Set(mine.filter((ref) => ref.override).map((ref) => ref.step));
    const label = `${steps.size} step${steps.size === 1 ? '' : 's'}`;
    return overrides.size > 0 ? `${label} (${overrides.size} with overrides)` : label;
  };

  const save = () => {
    if (!draft) return;
    const name = draft.name.trim();
    if (!name) {
      toast.error('A prompt name is required.');
      return;
    }
    if (name !== draft.originalName && definition.prompts[name]) {
      toast.error(`A prompt named '${name}' already exists.`);
      return;
    }
    let next = definition;
    const prompts = { ...definition.prompts };
    if (draft.originalName && draft.originalName !== name) {
      delete prompts[draft.originalName];
      // A rename rewrites every reference so nothing dangles.
      next = renameRefs(definition, draft.originalName, name);
    }
    prompts[name] = { text: draft.text, description: draft.description.trim() || undefined };
    onChange({ ...next, prompts });
    setDraft(null);
  };

  /** Copies drop the reserved 'default-' prefix so they read as ordinary entries. */
  const copyName = (name: string): string => {
    const base = `${name.replace(/^default-/, '')}-copy`;
    let copy = base;
    for (let n = 2; definition.prompts[copy]; n++) copy = `${base}-${n}`;
    return copy;
  };

  const duplicate = (name: string) => {
    onChange({
      ...definition,
      prompts: { ...definition.prompts, [copyName(name)]: { ...definition.prompts[name] } },
    });
  };

  /** Materialize a modifiable copy of a built-in prompt; the built-in itself stays untouched. */
  const duplicateBuiltin = (name: string) => {
    onChange({
      ...definition,
      prompts: {
        ...definition.prompts,
        [copyName(name)]: {
          text: V2_DEFAULT_PROMPTS[name].text,
          description: `Copy of the built-in ${name} prompt.`,
        },
      },
    });
  };

  const remove = (name: string) => {
    const used = referencedBy(name);
    const prompts = { ...definition.prompts };
    delete prompts[name];
    onChange({ ...definition, prompts });
    if (used !== 'unused')
      toast.warn(`Prompt '${name}' was referenced by ${used}; those references now dangle.`);
  };

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
    setDraft((current) =>
      current ? { ...current, text: `${current.text}<p>${token}</p>` } : current,
    );
  };

  return (
    <Col className="v2-prompt-library" gap="0.25rem">
      <p className="section-help-text">
        Shared prompt text is stored once and referenced by steps and actions; a step stores only
        its override. <code>{'{lookup:…}'}</code> tokens inject enabled reference data at
        composition.
      </p>
      <table className="v2-table v2-library-table">
        <thead>
          <tr>
            <th className="v2-col-name">Name</th>
            <th>Description</th>
            <th className="v2-col-refs">Referenced By</th>
            <th className="v2-col-actions">
              <button
                type="button"
                className="rule-icon-button"
                aria-label="Add a prompt"
                title="Add a prompt"
                onClick={() =>
                  setDraft({ originalName: null, name: '', description: '', text: '' })
                }
              >
                <FaPlus />
              </button>
            </th>
          </tr>
        </thead>
        <tbody>
          <Show visible={names.length === 0 && missingDefaults.length === 0}>
            <tr>
              <td colSpan={4} className="v2-library-empty">
                No prompts yet — add one and reference it from an analysis.
              </td>
            </tr>
          </Show>
          {missingDefaults.map((name) => (
            <tr key={name} className="v2-builtin-row">
              <td className="v2-col-name">
                <span className="v2-prompt-name">{name}</span>
              </td>
              <td>{V2_DEFAULT_PROMPTS[name].description}</td>
              <td className="v2-col-refs">built-in default</td>
              <td className="v2-col-actions">
                <button
                  type="button"
                  className="rule-icon-button"
                  aria-label={`Customize the built-in prompt '${name}'`}
                  title="Customize this built-in prompt"
                  onClick={() =>
                    setDraft({
                      originalName: null,
                      name,
                      description: V2_DEFAULT_PROMPTS[name].description,
                      text: V2_DEFAULT_PROMPTS[name].text,
                    })
                  }
                >
                  <FaEdit />
                </button>
                <button
                  type="button"
                  className="rule-icon-button"
                  aria-label={`Copy the built-in prompt '${name}'`}
                  title="Create an editable copy (the built-in stays untouched)"
                  onClick={() => duplicateBuiltin(name)}
                >
                  <FaCopy />
                </button>
              </td>
            </tr>
          ))}
          {names.map((name) => (
            <tr key={name}>
              <td className="v2-col-name">
                <span className="v2-prompt-name">{name}</span>
              </td>
              <td>{definition.prompts[name].description ?? ''}</td>
              <td className="v2-col-refs">{referencedBy(name)}</td>
              <td className="v2-col-actions">
                <button
                  type="button"
                  className="rule-icon-button"
                  aria-label={`Edit prompt '${name}'`}
                  title={`Edit prompt '${name}'`}
                  onClick={() =>
                    setDraft({
                      originalName: name,
                      name,
                      description: definition.prompts[name].description ?? '',
                      text: definition.prompts[name].text,
                    })
                  }
                >
                  <FaEdit />
                </button>
                <button
                  type="button"
                  className="rule-icon-button"
                  aria-label={`Duplicate prompt '${name}'`}
                  title={`Duplicate prompt '${name}'`}
                  onClick={() => duplicate(name)}
                >
                  <FaCopy />
                </button>
                <button
                  type="button"
                  className="rule-icon-button delete"
                  aria-label={`Delete prompt '${name}'`}
                  title={`Delete prompt '${name}'`}
                  onClick={() => remove(name)}
                >
                  <FaTrash />
                </button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>

      <Modal
        headerText={draft?.originalName ? 'Edit Prompt' : 'Add Prompt'}
        isShowing={!!draft}
        hide={() => setDraft(null)}
        type="custom"
        component={
          <div className="rule-modal-content v2-prompt-modal" ref={editorRef}>
            <Text
              name="prompt-name"
              label="Name"
              required
              value={draft?.name ?? ''}
              onChange={(e) => setDraft((d) => (d ? { ...d, name: e.target.value } : d))}
            />
            <TextArea
              name="prompt-description"
              label="Description"
              rows={2}
              value={draft?.description ?? ''}
              onChange={(e) => setDraft((d) => (d ? { ...d, description: e.target.value } : d))}
            />
            <Wysiwyg
              className="modal-wysiwyg"
              name="prompt-text"
              label="Prompt Text"
              value={draft?.text ?? ''}
              onChange={(text) => setDraft((d) => (d ? { ...d, text: text ?? '' } : d))}
            />
            <p className="v2-token-help">
              <strong>Insert Data Token</strong> — the prompt is exactly what is sent to the LLM;
              tokens mark where data is inserted, replaced once per item.{' '}
              <code>{'{content.*}'}</code> is the item being processed (its working copy, changes
              included). In Detect Duplicate prompts <code>{'{candidates}'}</code> inserts the
              compared stories and <code>{'{candidate.*}'}</code> single fields (iterate mode); the
              prompt must place them itself — see default-dedupe for the layout.{' '}
              <code>{'{lookup:*}'}</code> inserts reference lists (identical for every item).
              Analyses only: with no <code>{'{content}'}</code> token, the story is appended as a
              final '## News Story' section.
            </p>
            <label className="v2-token-group-label">Lookups</label>
            <div className="v2-token-list">
              {V2_LOOKUP_TOKENS.map(({ token, hint }) => (
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
            <label className="v2-token-group-label">Content (the item being processed)</label>
            <div className="v2-token-list">
              {V2_CONTENT_TOKENS.map(({ token, hint }) => (
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
            <label className="v2-token-group-label">Candidate (Detect Duplicate prompts)</label>
            <div className="v2-token-list">
              {V2_CANDIDATE_TOKENS.map(({ token, hint }) => (
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
          </div>
        }
        customButtons={
          <Row justifyContent="flex-end" gap="0.5rem" width="100%">
            <Button variant={ButtonVariant.secondary} onClick={() => setDraft(null)}>
              Cancel
            </Button>
            <Button onClick={save}>Save</Button>
          </Row>
        }
      />
    </Col>
  );
};
