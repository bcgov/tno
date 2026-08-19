import React from 'react';
import { FaChevronDown, FaChevronRight, FaPlus, FaTrash } from 'react-icons/fa';
import { toast } from 'react-toastify';
import { Button, ButtonVariant, Col, type IOptionItem, Row, Show, Text, TextArea } from 'tno-core';

import {
  collectV2CollectionNames,
  createDefaultV2Step,
  parseV2Definition,
  serializeV2Definition,
} from './constants';
import {
  type IV2ActionDescriptor,
  type IV2Definition,
  type IV2Step,
  type IV2ValidationError,
} from './interfaces';
import { V2StepEditor } from './V2StepEditor';

export interface IV2DesignerProps {
  /** The profile's definition document as raw JSON. */
  value?: string | null;
  onChange: (definition: string) => void;
  descriptors: IV2ActionDescriptor[];
  filterOptions: IOptionItem[];
  llmOptions: IOptionItem[];
  reportOptions: IOptionItem[];
  notificationOptions: IOptionItem[];
  actionOptions: IOptionItem[];
  /** Validate the current definition against the catalog (server-side). */
  onValidate?: (definition: string) => Promise<IV2ValidationError[]>;
}

/**
 * The v2 profile designer: the prompt library (shared text stored once, referenced by steps),
 * steps grouped by lifecycle phase, and on-demand validation with per-path findings. A raw JSON
 * editor is available for power edits; the same document round-trips both ways.
 */
export const V2Designer: React.FC<IV2DesignerProps> = ({
  value,
  onChange,
  descriptors,
  filterOptions,
  llmOptions,
  reportOptions,
  notificationOptions,
  actionOptions,
  onValidate,
}) => {
  const definition = React.useMemo(() => parseV2Definition(value), [value]);
  const [collapsed, setCollapsed] = React.useState<Set<number>>(new Set());
  const [expandedPrompts, setExpandedPrompts] = React.useState<Set<string>>(new Set());
  const [newPromptName, setNewPromptName] = React.useState('');
  const [showJson, setShowJson] = React.useState(false);
  const [jsonDraft, setJsonDraft] = React.useState('');
  const [findings, setFindings] = React.useState<IV2ValidationError[] | null>(null);

  const update = (next: IV2Definition) => onChange(serializeV2Definition(next));
  const promptNames = Object.keys(definition.prompts);
  const collectionNames = collectV2CollectionNames(definition);

  const setStep = (index: number, step: IV2Step) => {
    const steps = [...definition.steps];
    steps[index] = step;
    update({ ...definition, steps });
  };

  const moveStep = (index: number, direction: -1 | 1) => {
    const target = index + direction;
    if (target < 0 || target >= definition.steps.length) return;
    const steps = [...definition.steps];
    const [moved] = steps.splice(index, 1);
    steps.splice(target, 0, moved);
    update({ ...definition, steps });
  };

  const toggleCollapsed = (index: number) =>
    setCollapsed((current) => {
      const next = new Set(current);
      if (next.has(index)) next.delete(index);
      else next.add(index);
      return next;
    });

  return (
    <Col className="v2-designer" gap="0.75rem">
      <Row className="section-header" nowrap>
        <h2>Prompt Library</h2>
      </Row>
      <p className="section-help-text">
        Shared prompt text lives here once and is referenced by analyses. A step stores only its
        override — differences between steps are visible instead of hiding in near-identical copies.
        Prompts support tokens: {'{content}'}, {'{content.field}'}, {'{lookup:tags}'},{' '}
        {'{collection:$run.name}'}.
      </p>
      {promptNames.map((name) => (
        <Col key={name} className="v2-list-item" gap="0.25rem">
          <Row gap="0.5rem" alignItems="center" nowrap>
            <button
              type="button"
              className="rule-icon-button"
              title={expandedPrompts.has(name) ? 'Collapse' : 'Expand'}
              onClick={() =>
                setExpandedPrompts((current) => {
                  const next = new Set(current);
                  if (next.has(name)) next.delete(name);
                  else next.add(name);
                  return next;
                })
              }
            >
              {expandedPrompts.has(name) ? <FaChevronDown /> : <FaChevronRight />}
            </button>
            <strong>{name}</strong>
            <span className="v2-field-help">
              {definition.prompts[name].length.toLocaleString()} chars
            </span>
            <button
              type="button"
              className="rule-icon-button delete"
              title={`Delete prompt '${name}'`}
              onClick={() => {
                const prompts = { ...definition.prompts };
                delete prompts[name];
                update({ ...definition, prompts });
              }}
            >
              <FaTrash />
            </button>
          </Row>
          <Show visible={expandedPrompts.has(name)}>
            <TextArea
              name={`prompt-${name}`}
              rows={6}
              value={definition.prompts[name]}
              onChange={(e) =>
                update({
                  ...definition,
                  prompts: { ...definition.prompts, [name]: e.target.value },
                })
              }
            />
          </Show>
        </Col>
      ))}
      <Row gap="0.5rem" alignItems="flex-end" nowrap>
        <Text
          name="new-prompt-name"
          label="New prompt name"
          width="16rem"
          value={newPromptName}
          onChange={(e) => setNewPromptName(e.target.value)}
        />
        <Button
          variant={ButtonVariant.secondary}
          disabled={!newPromptName.trim() || !!definition.prompts[newPromptName.trim()]}
          onClick={() => {
            const name = newPromptName.trim();
            update({ ...definition, prompts: { ...definition.prompts, [name]: '' } });
            setExpandedPrompts((current) => new Set(current).add(name));
            setNewPromptName('');
          }}
        >
          <FaPlus /> Add prompt
        </Button>
      </Row>

      <Row className="section-header" nowrap>
        <h2>Steps</h2>
      </Row>
      <p className="section-help-text">
        Steps run in order within their phase: initialize (once) → process (per content item) →
        complete (once). Every action in a step applies to the item the step iterates; to act on
        different content, iterate a different collection.
      </p>
      {definition.steps.map((step, index) => (
        <Col key={index} className="v2-step-card" gap="0.5rem">
          <Row gap="0.5rem" alignItems="center" nowrap className="v2-step-card-header">
            <button
              type="button"
              className="rule-icon-button"
              title={collapsed.has(index) ? 'Expand' : 'Collapse'}
              onClick={() => toggleCollapsed(index)}
            >
              {collapsed.has(index) ? <FaChevronRight /> : <FaChevronDown />}
            </button>
            <span className={`v2-badge v2-phase-${step.phase}`}>{step.phase}</span>
            <strong>{step.name || '(unnamed step)'}</strong>
            <span className="v2-field-help">
              {step.analyses.length} analysis(es), {step.actions.length} action(s)
              {step.isEnabled ? '' : ' — disabled'}
            </span>
            <button
              type="button"
              className="rule-icon-button"
              title="Move up"
              disabled={index === 0}
              onClick={() => moveStep(index, -1)}
            >
              ↑
            </button>
            <button
              type="button"
              className="rule-icon-button"
              title="Move down"
              disabled={index === definition.steps.length - 1}
              onClick={() => moveStep(index, 1)}
            >
              ↓
            </button>
            <button
              type="button"
              className="rule-icon-button delete"
              title="Delete step"
              onClick={() =>
                update({ ...definition, steps: definition.steps.filter((_, i) => i !== index) })
              }
            >
              <FaTrash />
            </button>
          </Row>
          <Show visible={!collapsed.has(index)}>
            <V2StepEditor
              step={step}
              descriptors={descriptors}
              collectionNames={collectionNames}
              promptNames={promptNames}
              filterOptions={filterOptions}
              llmOptions={llmOptions}
              reportOptions={reportOptions}
              notificationOptions={notificationOptions}
              actionOptions={actionOptions}
              onChange={(next) => setStep(index, next)}
            />
          </Show>
        </Col>
      ))}
      <Row gap="0.5rem">
        <Button
          variant={ButtonVariant.secondary}
          onClick={() =>
            update({ ...definition, steps: [...definition.steps, createDefaultV2Step('process')] })
          }
        >
          <FaPlus /> Add step
        </Button>
      </Row>

      <Row className="section-header" nowrap>
        <h2>Validation</h2>
      </Row>
      <Row gap="0.5rem">
        <Show visible={!!onValidate}>
          <Button
            variant={ButtonVariant.secondary}
            onClick={async () => {
              try {
                const results = await onValidate!(serializeV2Definition(definition));
                setFindings(results);
                if (results.length === 0) toast.success('The definition is valid.');
              } catch {
                toast.error('Validation failed to run.');
              }
            }}
          >
            Validate
          </Button>
        </Show>
        <Button
          variant={ButtonVariant.link}
          onClick={() => {
            setJsonDraft(JSON.stringify(definition, undefined, 2));
            setShowJson((current) => !current);
          }}
        >
          {showJson ? 'Hide JSON' : 'Edit as JSON'}
        </Button>
      </Row>
      <Show visible={findings !== null && (findings ?? []).length > 0}>
        <Col className="v2-findings" gap="0.25rem">
          {(findings ?? []).map((finding, index) => (
            <Row key={index} gap="0.5rem" nowrap>
              <span
                className={`v2-badge ${
                  finding.severity === 'error' ? 'v2-badge-danger' : 'v2-badge-warning'
                }`}
              >
                {finding.severity}
              </span>
              <code>{finding.path}</code>
              <span>{finding.message}</span>
            </Row>
          ))}
        </Col>
      </Show>
      <Show visible={showJson}>
        <Col gap="0.5rem">
          <TextArea
            name="definition-json"
            rows={20}
            value={jsonDraft}
            onChange={(e) => setJsonDraft(e.target.value)}
          />
          <Row gap="0.5rem">
            <Button
              variant={ButtonVariant.secondary}
              onClick={() => {
                try {
                  const parsed = JSON.parse(jsonDraft);
                  onChange(JSON.stringify(parsed));
                  toast.success('Definition updated from JSON.');
                } catch (error) {
                  toast.error(`The JSON is not valid: ${(error as Error).message}`);
                }
              }}
            >
              Apply JSON
            </Button>
          </Row>
        </Col>
      </Show>
    </Col>
  );
};
