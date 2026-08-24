import React from 'react';
import { FaPlus, FaTrash } from 'react-icons/fa';
import {
  Checkbox,
  Col,
  type IOptionItem,
  Row,
  Select,
  Show,
  Text,
  TextArea,
  Wysiwyg,
} from 'tno-core';

import { createOption, findOptionByValue, toNumberOrUndefined } from '../utils';
import { ComboBox } from './ComboBox';
import { fitSelectWidth } from './constants';
import { type IAutomationAnalysis } from './interfaces';
import { PromptTokens } from './PromptTokens';

export interface IAnalysisEditorProps {
  analysis: IAutomationAnalysis;
  /** Names of analyses declared earlier in the step (valid chain targets). */
  earlierNames: string[];
  /** Drafts the step's content.create actions declare (valid '{target}' sources). */
  draftNames: string[];
  promptNames: string[];
  llmOptions: IOptionItem[];
  onChange: (analysis: IAutomationAnalysis) => void;
}

/** Common return type specs offered by the Type autocomplete; freeform (e.g. 'int(-5..5)')
 * stays allowed. */
const RETURN_TYPES = ['string', 'string?', 'string[]', 'bool', 'int', 'int(-5..5)'];

/**
 * Editor for one named analysis: its prompt (library reference plus override, or inline text),
 * its declared result shape, optional chaining, and raw mode. One analysis covering several
 * properties shares a single LLM call; one per property keeps a complex prompt isolated.
 */
export const AnalysisEditor: React.FC<IAnalysisEditorProps> = ({
  analysis,
  earlierNames,
  draftNames,
  promptNames,
  llmOptions,
  onChange,
}) => {
  const overrideRef = React.useRef<HTMLDivElement>(null);
  const promptOptions = [
    createOption('(inline text)', ''),
    ...promptNames.map((name) => createOption(name, name)),
  ];
  const chainOptions = [
    createOption('(none)', ''),
    ...earlierNames.map((name) => createOption(name, name)),
  ];
  // The draft an analysis reads through '{target...}'. Every draft the step creates is offered:
  // an analysis runs where the action consuming it runs, not at a position of its own.
  const targetOptions = [
    createOption('(none)', ''),
    ...draftNames.map((name) => createOption(name.replace(/^\$item\./, ''), name)),
  ];
  const llmSelectOptions = [createOption('step default', ''), ...llmOptions];
  const returnEntries = Object.entries(analysis.returns ?? {});

  const set = (values: Partial<IAutomationAnalysis>) => onChange({ ...analysis, ...values });

  return (
    <Col className="automation-analysis-editor" gap="0.5rem">
      <Row gap="1rem" alignItems="flex-end" nowrap>
        <Text
          name="analysis-name"
          label="Name"
          required
          width="12rem"
          value={analysis.name}
          onChange={(e) => set({ name: e.target.value })}
        />
        <Select
          name="analysis-prompt-ref"
          label="Prompt"
          width={fitSelectWidth(['(inline text)', ...promptNames])}
          isClearable={false}
          options={promptOptions}
          value={findOptionByValue(promptOptions, analysis.prompt?.ref ?? '')}
          onChange={(newValue) => {
            const option = newValue as IOptionItem;
            const ref = option?.value ? `${option.value}` : null;
            set({ prompt: { ...analysis.prompt, ref, text: ref ? null : analysis.prompt?.text } });
          }}
        />
        <Select
          name="analysis-chain"
          label="Chain"
          width={fitSelectWidth(['(none)', ...earlierNames])}
          isClearable={false}
          options={chainOptions}
          value={findOptionByValue(chainOptions, analysis.chain ?? '')}
          onChange={(newValue) => {
            const option = newValue as IOptionItem;
            set({ chain: option?.value ? `${option.value}` : null });
          }}
        />
      </Row>
      <Row gap="1rem" alignItems="flex-end" nowrap>
        <Show visible={draftNames.length > 0}>
          <Select
            name="analysis-target"
            label="Target"
            tooltip="The draft this analysis's '{target...}' tokens read, so the prompt can see what earlier actions put on the copy. '{content...}' always reads the item the iteration started from."
            width={fitSelectWidth(['(none)', ...draftNames])}
            isClearable={false}
            options={targetOptions}
            value={findOptionByValue(targetOptions, analysis.target ?? '')}
            onChange={(newValue) => {
              const option = newValue as IOptionItem;
              set({ target: option?.value ? `${option.value}` : null });
            }}
          />
        </Show>
        <Select
          name="analysis-llm"
          label="LLM Override"
          width={fitSelectWidth(['step default', ...llmOptions.map((option) => `${option.label}`)])}
          isClearable={false}
          options={llmSelectOptions}
          value={findOptionByValue(llmOptions, analysis.llmId) ?? createOption('step default', '')}
          onChange={(newValue) => set({ llmId: toNumberOrUndefined(newValue as IOptionItem) })}
        />
        <div className="checkbox-inline">
          <Checkbox
            name="analysis-raw"
            label="Raw response"
            tooltip="Keep the response as plain text; actions gate on it with confirmation statements (how migrated v1 actions work)."
            checked={analysis.raw ?? false}
            onChange={(e) => set({ raw: e.target.checked })}
          />
        </div>
      </Row>
      <Show visible={!!analysis.target}>
        <p className="automation-field-help">
          <code>{'{target}'}</code> and <code>{'{target.field}'}</code> read{' '}
          <code>{analysis.target}</code> — the copy this step&apos;s actions are building, including
          the changes they have already made to it. <code>{'{content}'}</code> and{' '}
          <code>{'{content.field}'}</code> keep reading the item the iteration started from.
        </p>
      </Show>
      <Show visible={!analysis.prompt?.ref}>
        <TextArea
          name="analysis-prompt-text"
          label="Prompt Text"
          rows={4}
          placeholder="The prompt sent for this analysis"
          value={analysis.prompt?.text ?? ''}
          onChange={(e) => set({ prompt: { ...analysis.prompt, text: e.target.value } })}
        />
      </Show>
      <Show visible={!!analysis.prompt?.ref}>
        <div ref={overrideRef}>
          <Wysiwyg
            className="modal-wysiwyg"
            name="analysis-prompt-override"
            label="Prompt Override"
            value={analysis.prompt?.override ?? ''}
            onChange={(text) => set({ prompt: { ...analysis.prompt, override: text || null } })}
          />
        </div>
        <PromptTokens
          editorRef={overrideRef}
          showTarget={draftNames.length > 0}
          onAppend={(token) =>
            set({
              prompt: {
                ...analysis.prompt,
                override: `${analysis.prompt?.override ?? ''}<p>${token}</p>`,
              },
            })
          }
        />
      </Show>
      <Show visible={!analysis.raw}>
        <div className="frm-in">
          <label>Returns</label>
          <p className="automation-field-help">
            The keys this analysis answers with. The engine asks for JSON in exactly this shape and
            validates the reply, then actions read a key as{' '}
            <code>{analysis.name || '<analysis>'}. &lt;key&gt;</code> in their Value field. To score
            stories, declare an integer key with a range — e.g. key <code>score</code> of type{' '}
            <code>int(1..10)</code> — and point a Score Content action&apos;s Value at{' '}
            <code>{`${analysis.name || '<analysis>'}.score`}</code>.
          </p>
          <div className="automation-returns">
            <div className="automation-returns-head">
              <span>Key</span>
              <span>Type</span>
              <button
                type="button"
                className="automation-returns-add"
                title="Add a return key"
                onClick={() => set({ returns: { ...(analysis.returns ?? {}), '': 'string' } })}
              >
                <FaPlus />
              </button>
            </div>
            {returnEntries.map(([key, type], index) => (
              <div key={index} className="automation-returns-row">
                <Text
                  name={`analysis-return-key-${index}`}
                  placeholder="key"
                  width="100%"
                  value={key}
                  onChange={(e) => {
                    const updated = returnEntries.map(([k, t], i) =>
                      i === index ? ([e.target.value, t] as const) : ([k, t] as const),
                    );
                    set({ returns: Object.fromEntries(updated) });
                  }}
                />
                <ComboBox
                  name={`analysis-return-type-${index}`}
                  placeholder="type"
                  suggestions={RETURN_TYPES}
                  value={type}
                  onChange={(next) => {
                    const updated = returnEntries.map(([k, t], i) =>
                      i === index ? ([k, next] as const) : ([k, t] as const),
                    );
                    set({ returns: Object.fromEntries(updated) });
                  }}
                />
                <button
                  type="button"
                  className="automation-returns-del"
                  title="Remove this return key"
                  onClick={() =>
                    set({
                      returns: Object.fromEntries(returnEntries.filter((_, i) => i !== index)),
                    })
                  }
                >
                  <FaTrash />
                </button>
              </div>
            ))}
          </div>
        </div>
      </Show>
    </Col>
  );
};
