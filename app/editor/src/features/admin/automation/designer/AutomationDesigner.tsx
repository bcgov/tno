import React from 'react';
import { DragDropContext, Draggable, type DropResult } from 'react-beautiful-dnd';
import {
  FaChevronDown,
  FaChevronRight,
  FaCopy,
  FaEdit,
  FaFlask,
  FaGripLines,
  FaPlus,
  FaTrash,
} from 'react-icons/fa';
import { toast } from 'react-toastify';
import { Button, ButtonVariant, Col, type IOptionItem, Modal, Row, Show, TextArea } from 'tno-core';

import { SectionInfoButton } from '../SectionInfoButton';
import { StrictModeDroppable } from '../StrictModeDroppable';
import { findOptionByValue } from '../utils';
import { ActionEditor } from './ActionEditor';
import { AnalysisEditor } from './AnalysisEditor';
import {
  collectCollectionNames,
  createDefaultAction,
  createDefaultStep,
  parseDefinition,
  serializeDefinition,
} from './constants';
import {
  type IAutomationAction,
  type IAutomationActionDescriptor,
  type IAutomationAnalysis,
  type IAutomationDefinition,
  type IAutomationStep,
  type IAutomationValidationError,
} from './interfaces';
import { PromptLibrary } from './PromptLibrary';
import { StepEditor } from './StepEditor';

export interface IAutomationDesignerProps {
  /** The profile's definition document as raw JSON. */
  value?: string | null;
  onChange: (definition: string) => void;
  descriptors: IAutomationActionDescriptor[];
  filterOptions: IOptionItem[];
  llmOptions: IOptionItem[];
  reportOptions: IOptionItem[];
  notificationOptions: IOptionItem[];
  actionOptions: IOptionItem[];
  /** Validate the current definition against the catalog (server-side). */
  onValidate?: (definition: string) => Promise<IAutomationValidationError[]>;
}

interface IStepModalState {
  /** The step index being edited, or null when adding. */
  index: number | null;
  draft: IAutomationStep;
}

interface IActionModalState {
  stepIndex: number;
  /** The action index being edited, or null when adding. */
  index: number | null;
  draft: IAutomationAction;
}

interface IAnalysisModalState {
  stepIndex: number;
  /** The analysis index being edited, or null when adding. */
  index: number | null;
  draft: IAutomationAnalysis;
}

const deepCopy = <T,>(value: T): T => JSON.parse(JSON.stringify(value)) as T;

/**
 * The profile designer: the prompt library, a steps grid (name, phase, source, counts - rows
 * drag to reorder, chevrons expand to the step's analyses and actions, pencils open modal forms),
 * and on-demand validation with per-path findings. A raw JSON editor is available for power
 * edits; the same document round-trips both ways.
 */
export const AutomationDesigner: React.FC<IAutomationDesignerProps> = ({
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
  const definition = React.useMemo(() => parseDefinition(value), [value]);
  const [expanded, setExpanded] = React.useState<Set<number>>(new Set());
  const [showJson, setShowJson] = React.useState(false);
  const [jsonDraft, setJsonDraft] = React.useState('');
  const [findings, setFindings] = React.useState<IAutomationValidationError[] | null>(null);
  const [stepModal, setStepModal] = React.useState<IStepModalState | null>(null);
  const [actionModal, setActionModal] = React.useState<IActionModalState | null>(null);
  // Collapsed page sections.
  const [collapsedSections, setCollapsedSections] = React.useState<Set<string>>(new Set());
  const toggleSection = (name: string) =>
    setCollapsedSections((previous) => {
      const next = new Set(previous);
      if (next.has(name)) next.delete(name);
      else next.add(name);
      return next;
    });
  // Collapsed step groups, keyed by group name (all runs of the name collapse together).
  const [collapsedGroups, setCollapsedGroups] = React.useState<Set<string>>(new Set());
  const toggleGroup = (name: string) =>
    setCollapsedGroups((previous) => {
      const next = new Set(previous);
      if (next.has(name)) next.delete(name);
      else next.add(name);
      return next;
    });
  const [analysisModal, setAnalysisModal] = React.useState<IAnalysisModalState | null>(null);

  const update = (next: IAutomationDefinition) => onChange(serializeDefinition(next));
  const promptNames = Object.keys(definition.prompts);
  const collectionNames = collectCollectionNames(definition);

  const setStep = (index: number, step: IAutomationStep) => {
    const steps = [...definition.steps];
    steps[index] = step;
    update({ ...definition, steps });
  };

  const anyExpanded = expanded.size > 0;
  const toggleExpanded = (index: number) =>
    setExpanded((current) => {
      const next = new Set(current);
      if (next.has(index)) next.delete(index);
      else next.add(index);
      return next;
    });

  // One drag context covers both lists; distinct droppable ids keep steps and actions (and
  // actions across steps) from mixing.
  const DragDropContextAny = DragDropContext as any;
  const DroppableAny = StrictModeDroppable as any;
  const DraggableAny = Draggable as any;

  const onDragEnd = (result: DropResult) => {
    const { source, destination } = result;
    if (!destination) return;
    if (source.droppableId !== destination.droppableId) return;
    if (source.index === destination.index) return;

    if (source.droppableId === 'automation-steps') {
      const steps = [...definition.steps];
      const [moved] = steps.splice(source.index, 1);
      steps.splice(destination.index, 0, moved);
      update({ ...definition, steps });
      // Move the expanded flags the same way so they follow their steps.
      setExpanded((current) => {
        const flags = definition.steps.map((_, i) => current.has(i));
        const [movedFlag] = flags.splice(source.index, 1);
        flags.splice(destination.index, 0, movedFlag);
        return new Set(flags.flatMap((isExpanded, i) => (isExpanded ? [i] : [])));
      });
      return;
    }

    if (source.droppableId.startsWith('automation-actions-')) {
      const stepIndex = Number(source.droppableId.replace('automation-actions-', ''));
      const step = definition.steps[stepIndex];
      if (!step) return;
      const actions = [...step.actions];
      const [moved] = actions.splice(source.index, 1);
      actions.splice(destination.index, 0, moved);
      setStep(stepIndex, { ...step, actions });
    }
  };

  const sourceLabel = (step: IAutomationStep): React.ReactNode => {
    if (step.phase === 'init' || !step.source) return '—';
    const source = step.source;
    if (source?.from === 'collection')
      return <span className="automation-prompt-name">{source.collection ?? '?'}</span>;
    if (source?.from === 'filter')
      return findOptionByValue(filterOptions, source.filter)?.label ?? `filter ${source.filter}`;
    // Legacy documents may still carry an unsupported source kind; validation flags it.
    return source?.from ? `${source.from} (unsupported)` : '—';
  };

  const typeLabel = (type: string): string =>
    descriptors.find((descriptor) => descriptor.type === type)?.label ?? type;

  const runsWhenLabel = (action: IAutomationAction): React.ReactNode => {
    // Dedupe routing gates read as sentences, matching the Runs when options.
    const fromRef = action.when?.from;
    if (fromRef && fromRef.endsWith('.isDuplicate'))
      return `'${fromRef.replace(/\.isDuplicate$/, '')}' found a duplicate`;
    const notFrom = action.when?.not?.from;
    if (notFrom && notFrom.endsWith('.isDuplicate'))
      return `'${notFrom.replace(/\.isDuplicate$/, '')}' found no duplicate`;
    if (action.confirm) return 'LLM confirmation';
    // An Analysis answer gate is a condition; show which answer it reads.
    if (fromRef) return `when '${fromRef}'`;
    if (notFrom) return `unless '${notFrom}'`;
    if (action.when) return 'Condition';
    return <span className="automation-muted">Always run</span>;
  };

  const duplicateStep = (index: number) => {
    const steps = [...definition.steps];
    const copy = deepCopy(steps[index]);
    copy.name = `${copy.name} (copy)`;
    steps.splice(index + 1, 0, copy);
    update({ ...definition, steps });
  };

  /** Drafts declared by content.create actions before the given index (modal draft pickers). */
  const draftNamesBefore = (step: IAutomationStep, index: number | null): string[] =>
    step.actions
      .slice(0, index ?? step.actions.length)
      .filter((action) => action.type === 'content.create' && !!action.as)
      .map((action) => action.as!);

  return (
    <Col className="automation-designer" gap="0.5rem">
      <Row className="section-header" nowrap>
        <Row className="section-header-title" nowrap>
          <button
            type="button"
            className="rule-icon-button"
            aria-expanded={!collapsedSections.has('prompts')}
            aria-label="Toggle the Prompt Library section"
            title={collapsedSections.has('prompts') ? 'Expand' : 'Collapse'}
            onClick={() => toggleSection('prompts')}
          >
            {collapsedSections.has('prompts') ? <FaChevronRight /> : <FaChevronDown />}
          </button>
          <h2 className="automation-section-toggle" onClick={() => toggleSection('prompts')}>
            Prompt Library
          </h2>
        </Row>
      </Row>
      <Show visible={!collapsedSections.has('prompts')}>
        <PromptLibrary definition={definition} onChange={update} />
      </Show>

      <Row className="section-header" nowrap>
        <Row className="section-header-title" nowrap>
          <button
            type="button"
            className="rule-icon-button"
            aria-expanded={!collapsedSections.has('steps')}
            aria-label="Toggle the Steps section"
            title={collapsedSections.has('steps') ? 'Expand' : 'Collapse'}
            onClick={() => toggleSection('steps')}
          >
            {collapsedSections.has('steps') ? <FaChevronRight /> : <FaChevronDown />}
          </button>
          <h2 className="automation-section-toggle" onClick={() => toggleSection('steps')}>
            Steps
          </h2>
          <SectionInfoButton
            title="Steps"
            content={
              <p>
                Steps run in phase order (init → process → complete) and in row order within a
                phase. Every action in a step applies to the item the step iterates; to act on
                different content, iterate a different collection.
              </p>
            }
          />
        </Row>
      </Row>
      <Show visible={!collapsedSections.has('steps')}>
        <p className="section-help-text">
          Init steps run once before iteration; each process step iterates its declared source;
          complete steps run once after. Drag rows to reorder within a phase.
          <br />
          <span className="help-accent">
            An analysis executes at most once per item, and only when a consuming action is
            reachable — actions gated by a property condition send no prompt.
          </span>
        </p>
        <div className="automation-grid">
          <Row className="automation-grid-header" nowrap>
            <span className="automation-gc-drag" />
            <span className="automation-gc-collapse">
              <button
                type="button"
                className="rule-icon-button"
                aria-label={anyExpanded ? 'Collapse all steps' : 'Expand all steps'}
                title={anyExpanded ? 'Collapse all steps' : 'Expand all steps'}
                onClick={() =>
                  setExpanded(
                    anyExpanded ? new Set() : new Set(definition.steps.map((_, index) => index)),
                  )
                }
              >
                {anyExpanded ? <FaChevronDown /> : <FaChevronRight />}
              </button>
            </span>
            <span className="automation-gc-name">Step Name</span>
            <span className="automation-gc-phase">Phase</span>
            <span className="automation-gc-source">Source</span>
            <span className="automation-gc-count">Actions</span>
            <span className="automation-gc-enabled">Enabled</span>
            <span className="automation-gc-actions">
              <button
                type="button"
                className="rule-icon-button"
                aria-label="Add a step"
                title="Add a step"
                onClick={() => setStepModal({ index: null, draft: createDefaultStep('process') })}
              >
                <FaPlus />
              </button>
            </span>
          </Row>
          <DragDropContextAny onDragEnd={onDragEnd}>
            <DroppableAny droppableId="automation-steps" type="automation-steps">
              {(provided: any) => (
                <div ref={provided.innerRef} {...provided.droppableProps}>
                  {definition.steps.map((step, index) => (
                    <DraggableAny key={`step-${index}`} draggableId={`step-${index}`} index={index}>
                      {(dragProvided: any, dragSnapshot: any) => (
                        <div
                          className={`automation-grid-item${
                            dragSnapshot.isDragging ? ' is-dragging' : ''
                          }${step.group ? ' automation-grouped' : ''}`}
                          ref={dragProvided.innerRef}
                          {...dragProvided.draggableProps}
                        >
                          {!!step.group &&
                            step.group !== definition.steps[index - 1]?.group &&
                            !dragSnapshot.isDragging && (
                              <button
                                type="button"
                                className="automation-group-band"
                                aria-expanded={!collapsedGroups.has(step.group)}
                                title={
                                  collapsedGroups.has(step.group)
                                    ? 'Expand this group'
                                    : 'Collapse this group'
                                }
                                onClick={() => toggleGroup(step.group!)}
                              >
                                {collapsedGroups.has(step.group) ? (
                                  <FaChevronRight />
                                ) : (
                                  <FaChevronDown />
                                )}{' '}
                                {step.group}
                              </button>
                            )}
                          <Show visible={!step.group || !collapsedGroups.has(step.group)}>
                            <Row className="automation-grid-row" nowrap>
                              <span
                                className="automation-gc-drag automation-drag-handle"
                                title="Drag to reorder"
                                {...dragProvided.dragHandleProps}
                              >
                                <FaGripLines />
                              </span>
                              <span className="automation-gc-collapse">
                                <button
                                  type="button"
                                  className="rule-icon-button"
                                  aria-label={expanded.has(index) ? 'Collapse' : 'Expand'}
                                  title={expanded.has(index) ? 'Collapse' : 'Expand'}
                                  onClick={() => toggleExpanded(index)}
                                >
                                  {expanded.has(index) ? <FaChevronDown /> : <FaChevronRight />}
                                </button>
                              </span>
                              <span
                                className="automation-gc-name automation-step-name-toggle"
                                role="button"
                                tabIndex={0}
                                title={
                                  expanded.has(index) ? 'Collapse this step' : 'Expand this step'
                                }
                                onClick={() => toggleExpanded(index)}
                                onKeyDown={(e) => {
                                  if (e.key === 'Enter' || e.key === ' ') {
                                    e.preventDefault();
                                    toggleExpanded(index);
                                  }
                                }}
                              >
                                {step.name || '(unnamed step)'}
                              </span>
                              <span className="automation-gc-phase">
                                <span className={`automation-badge automation-phase-${step.phase}`}>
                                  {step.phase}
                                </span>
                              </span>
                              <span className="automation-gc-source">{sourceLabel(step)}</span>
                              <span className="automation-gc-count">{step.actions.length}</span>
                              <span className="automation-gc-enabled">
                                <input
                                  type="checkbox"
                                  className="automation-enabled-toggle"
                                  aria-label={`Step '${step.name}' enabled`}
                                  title="Toggle this step"
                                  checked={step.isEnabled}
                                  onChange={(e) =>
                                    setStep(index, { ...step, isEnabled: e.target.checked })
                                  }
                                />
                              </span>
                              <span className="automation-gc-actions">
                                <button
                                  type="button"
                                  className="rule-icon-button"
                                  aria-label={`Edit step '${step.name}'`}
                                  title="Edit step"
                                  onClick={() => setStepModal({ index, draft: deepCopy(step) })}
                                >
                                  <FaEdit />
                                </button>
                                <button
                                  type="button"
                                  className="rule-icon-button"
                                  aria-label={`Duplicate step '${step.name}'`}
                                  title="Duplicate step"
                                  onClick={() => duplicateStep(index)}
                                >
                                  <FaCopy />
                                </button>
                                <button
                                  type="button"
                                  className="rule-icon-button delete"
                                  aria-label={`Delete step '${step.name}'`}
                                  title="Delete step"
                                  onClick={() =>
                                    update({
                                      ...definition,
                                      steps: definition.steps.filter((_, i) => i !== index),
                                    })
                                  }
                                >
                                  <FaTrash />
                                </button>
                              </span>
                            </Row>
                            <Show visible={expanded.has(index)}>
                              <div className="automation-grid-expanded">
                                <Show visible={step.phase !== 'init'}>
                                  <div className="automation-grid automation-subgrid">
                                    <Row className="automation-grid-header" nowrap>
                                      <span className="automation-gc-drag" />
                                      <span className="automation-gc-name">Analysis</span>
                                      <span className="automation-gc-source">Prompt</span>
                                      <span className="automation-gc-sm">Chain</span>
                                      <span className="automation-gc-sm">LLM Override</span>
                                      <span className="automation-gc-sm">Returns</span>
                                      <span className="automation-gc-actions">
                                        <button
                                          type="button"
                                          className="rule-icon-button"
                                          aria-label="Add an analysis"
                                          title="Add an analysis"
                                          onClick={() =>
                                            setAnalysisModal({
                                              stepIndex: index,
                                              index: null,
                                              draft: {
                                                name: '',
                                                prompt: { text: '' },
                                                returns: {},
                                              },
                                            })
                                          }
                                        >
                                          <FaPlus />
                                        </button>
                                      </span>
                                    </Row>
                                    <Show visible={step.analyses.length === 0}>
                                      <Row className="automation-grid-row" nowrap>
                                        <span className="automation-muted">
                                          No analyses — actions gated by property conditions run
                                          without any LLM call.
                                        </span>
                                      </Row>
                                    </Show>
                                    {step.analyses.map((analysis, analysisIndex) => (
                                      <Row
                                        key={analysisIndex}
                                        className={`automation-grid-row${
                                          analysisIndex % 2 ? ' automation-row-even' : ''
                                        }`}
                                        nowrap
                                      >
                                        <span className="automation-gc-drag automation-gc-icon">
                                          <FaFlask />
                                        </span>
                                        <span className="automation-gc-name">{analysis.name}</span>
                                        <span className="automation-gc-source">
                                          {analysis.prompt?.ref ? (
                                            <>
                                              <span className="automation-prompt-name">
                                                {analysis.prompt.ref}
                                              </span>
                                              {analysis.prompt.override ? (
                                                <span className="automation-override-note">
                                                  {' '}
                                                  + override
                                                </span>
                                              ) : null}
                                            </>
                                          ) : (
                                            <span className="automation-muted">(inline text)</span>
                                          )}
                                        </span>
                                        <span className="automation-gc-sm">
                                          {analysis.chain ?? (
                                            <span className="automation-muted">—</span>
                                          )}
                                        </span>
                                        <span className="automation-gc-sm">
                                          {findOptionByValue(llmOptions, analysis.llmId)?.label ?? (
                                            <span className="automation-muted">step default</span>
                                          )}
                                        </span>
                                        <span className="automation-gc-sm">
                                          {analysis.raw ? 'Raw response' : 'Configured fields'}
                                        </span>
                                        <span className="automation-gc-actions">
                                          <button
                                            type="button"
                                            className="rule-icon-button"
                                            aria-label={`Edit analysis '${analysis.name}'`}
                                            title="Edit analysis"
                                            onClick={() =>
                                              setAnalysisModal({
                                                stepIndex: index,
                                                index: analysisIndex,
                                                draft: deepCopy(analysis),
                                              })
                                            }
                                          >
                                            <FaEdit />
                                          </button>
                                          <button
                                            type="button"
                                            className="rule-icon-button delete"
                                            aria-label={`Delete analysis '${analysis.name}'`}
                                            title="Delete analysis"
                                            onClick={() =>
                                              setStep(index, {
                                                ...step,
                                                analyses: step.analyses.filter(
                                                  (_, i) => i !== analysisIndex,
                                                ),
                                              })
                                            }
                                          >
                                            <FaTrash />
                                          </button>
                                        </span>
                                      </Row>
                                    ))}
                                  </div>
                                </Show>

                                <div className="automation-grid automation-subgrid">
                                  <Row className="automation-grid-header" nowrap>
                                    <span className="automation-gc-drag" />
                                    <span className="automation-gc-name">Name</span>
                                    <span className="automation-gc-source">Action Type</span>
                                    <span className="automation-gc-sm">Runs When</span>
                                    <span className="automation-gc-name">Filter</span>
                                    <span className="automation-gc-name">Into Collection</span>
                                    <span className="automation-gc-on">Enabled</span>
                                    <span className="automation-gc-actions">
                                      <button
                                        type="button"
                                        className="rule-icon-button"
                                        aria-label="Add an action"
                                        title="Add an action"
                                        onClick={() =>
                                          setActionModal({
                                            stepIndex: index,
                                            index: null,
                                            draft: createDefaultAction(''),
                                          })
                                        }
                                      >
                                        <FaPlus />
                                      </button>
                                    </span>
                                  </Row>
                                  <DroppableAny
                                    droppableId={`automation-actions-${index}`}
                                    type={`automation-actions-${index}`}
                                  >
                                    {(actionsProvided: any) => (
                                      <div
                                        ref={actionsProvided.innerRef}
                                        {...actionsProvided.droppableProps}
                                      >
                                        {step.actions.map((action, actionIndex) => (
                                          <DraggableAny
                                            key={`action-${index}-${actionIndex}`}
                                            draggableId={`action-${index}-${actionIndex}`}
                                            index={actionIndex}
                                          >
                                            {(actionDrag: any, actionSnapshot: any) => (
                                              <div
                                                className={
                                                  actionSnapshot.isDragging
                                                    ? 'is-dragging'
                                                    : undefined
                                                }
                                                ref={actionDrag.innerRef}
                                                {...actionDrag.draggableProps}
                                              >
                                                <Row
                                                  className={`automation-grid-row${
                                                    actionIndex % 2 ? ' automation-row-even' : ''
                                                  }`}
                                                  nowrap
                                                >
                                                  <span
                                                    className="automation-gc-drag automation-drag-handle"
                                                    title="Drag to reorder"
                                                    {...actionDrag.dragHandleProps}
                                                  >
                                                    <FaGripLines />
                                                  </span>
                                                  <span className="automation-gc-name">
                                                    {action.name ?? typeLabel(action.type)}
                                                  </span>
                                                  <span className="automation-gc-source">
                                                    <span className="automation-prompt-name">
                                                      {action.type}
                                                    </span>
                                                  </span>
                                                  <span className="automation-gc-sm">
                                                    {runsWhenLabel(action)}
                                                  </span>
                                                  <span className="automation-gc-name">
                                                    {action.filter != null ? (
                                                      findOptionByValue(
                                                        filterOptions,
                                                        action.filter,
                                                      )?.label ?? `filter ${action.filter}`
                                                    ) : (
                                                      <span className="automation-muted">—</span>
                                                    )}
                                                  </span>
                                                  <span className="automation-gc-name">
                                                    {action.into ? (
                                                      <span className="automation-prompt-name">
                                                        {action.into}
                                                      </span>
                                                    ) : (
                                                      <span className="automation-muted">—</span>
                                                    )}
                                                  </span>
                                                  <span className="automation-gc-on">
                                                    <input
                                                      type="checkbox"
                                                      className="automation-enabled-toggle"
                                                      aria-label={`Action '${
                                                        action.name ?? action.type
                                                      }' enabled`}
                                                      title="Toggle this action"
                                                      checked={action.isEnabled}
                                                      onChange={(e) =>
                                                        setStep(index, {
                                                          ...step,
                                                          actions: step.actions.map((a, i) =>
                                                            i === actionIndex
                                                              ? {
                                                                  ...a,
                                                                  isEnabled: e.target.checked,
                                                                }
                                                              : a,
                                                          ),
                                                        })
                                                      }
                                                    />
                                                  </span>
                                                  <span className="automation-gc-actions">
                                                    <button
                                                      type="button"
                                                      className="rule-icon-button"
                                                      aria-label="Edit action"
                                                      title="Edit action"
                                                      onClick={() =>
                                                        setActionModal({
                                                          stepIndex: index,
                                                          index: actionIndex,
                                                          draft: deepCopy(action),
                                                        })
                                                      }
                                                    >
                                                      <FaEdit />
                                                    </button>
                                                    <button
                                                      type="button"
                                                      className="rule-icon-button delete"
                                                      aria-label="Delete action"
                                                      title="Delete action"
                                                      onClick={() =>
                                                        setStep(index, {
                                                          ...step,
                                                          actions: step.actions.filter(
                                                            (_, i) => i !== actionIndex,
                                                          ),
                                                        })
                                                      }
                                                    >
                                                      <FaTrash />
                                                    </button>
                                                  </span>
                                                </Row>
                                              </div>
                                            )}
                                          </DraggableAny>
                                        ))}
                                        {actionsProvided.placeholder}
                                      </div>
                                    )}
                                  </DroppableAny>
                                </div>
                              </div>
                            </Show>
                          </Show>
                        </div>
                      )}
                    </DraggableAny>
                  ))}
                  {provided.placeholder}
                </div>
              )}
            </DroppableAny>
          </DragDropContextAny>
          <Show visible={definition.steps.length === 0}>
            <Row className="automation-grid-row automation-library-empty" nowrap>
              <span>No steps configured.</span>
            </Row>
          </Show>
        </div>
      </Show>

      <Row className="section-header" nowrap>
        <h2>Validation</h2>
      </Row>
      <Row gap="0.5rem">
        <Show visible={!!onValidate}>
          <Button
            variant={ButtonVariant.secondary}
            onClick={async () => {
              try {
                const results = await onValidate!(serializeDefinition(definition));
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
        <Col className="automation-findings" gap="0.25rem">
          {(findings ?? []).map((finding, index) => (
            <Row key={index} gap="0.5rem" nowrap>
              <span
                className={`automation-badge ${
                  finding.severity === 'error'
                    ? 'automation-badge-danger'
                    : 'automation-badge-warning'
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

      <Modal
        headerText={stepModal?.index != null ? 'Edit Step' : 'Add Step'}
        isShowing={!!stepModal}
        hide={() => setStepModal(null)}
        type="custom"
        component={
          <div className="rule-modal-content">
            <Show visible={!!stepModal}>
              <StepEditor
                step={stepModal?.draft ?? createDefaultStep()}
                collectionNames={collectionNames}
                filterOptions={filterOptions}
                llmOptions={llmOptions}
                onChange={(draft) => setStepModal((state) => (state ? { ...state, draft } : state))}
              />
            </Show>
          </div>
        }
        customButtons={
          <Row justifyContent="flex-end" gap="0.5rem" width="100%">
            <Button
              className="header-btn-outline"
              variant={ButtonVariant.secondary}
              onClick={() => setStepModal(null)}
            >
              Cancel
            </Button>
            <Button
              className="header-btn-save"
              onClick={() => {
                if (!stepModal) return;
                if (!stepModal.draft.name.trim()) {
                  toast.error('A step name is required.');
                  return;
                }
                const draft = { ...stepModal.draft };
                // Init steps never declare a source; process steps require one; complete steps
                // may leave it unset (they run once) but a half-configured source is an error.
                const source = draft.source;
                const sourceIsSet =
                  !!source && (source.from === 'filter' ? !!source.filter : !!source.collection);
                if (draft.phase === 'init') draft.source = undefined;
                else if (!sourceIsSet && (draft.phase === 'process' || source != null)) {
                  toast.error(
                    draft.source?.from === 'filter'
                      ? 'A filter is required for the step source.'
                      : 'A collection is required for the step source.',
                  );
                  return;
                }
                const steps = [...definition.steps];
                if (stepModal.index != null) steps[stepModal.index] = draft;
                else steps.push(draft);
                update({ ...definition, steps });
                setStepModal(null);
              }}
            >
              {stepModal?.index != null ? 'Done' : 'Add'}
            </Button>
          </Row>
        }
      />

      <Modal
        headerText={analysisModal?.index != null ? 'Edit Analysis' : 'Add Analysis'}
        isShowing={!!analysisModal}
        hide={() => setAnalysisModal(null)}
        type="custom"
        component={
          <div className="rule-modal-content">
            <Show visible={!!analysisModal}>
              <AnalysisEditor
                analysis={analysisModal?.draft ?? { name: '', prompt: {}, returns: {} }}
                earlierNames={
                  analysisModal
                    ? definition.steps[analysisModal.stepIndex].analyses
                        .slice(0, analysisModal.index ?? undefined)
                        .map((analysis) => analysis.name)
                    : []
                }
                promptNames={promptNames}
                llmOptions={llmOptions}
                onChange={(draft) =>
                  setAnalysisModal((state) => (state ? { ...state, draft } : state))
                }
              />
            </Show>
          </div>
        }
        customButtons={
          <Row justifyContent="flex-end" gap="0.5rem" width="100%">
            <Button
              className="header-btn-outline"
              variant={ButtonVariant.secondary}
              onClick={() => setAnalysisModal(null)}
            >
              Cancel
            </Button>
            <Button
              className="header-btn-save"
              onClick={() => {
                if (!analysisModal) return;
                if (!analysisModal.draft.name.trim()) {
                  toast.error('An analysis name is required.');
                  return;
                }
                const step = definition.steps[analysisModal.stepIndex];
                const analyses = [...step.analyses];
                if (analysisModal.index != null)
                  analyses[analysisModal.index] = analysisModal.draft;
                else analyses.push(analysisModal.draft);
                setStep(analysisModal.stepIndex, { ...step, analyses });
                setAnalysisModal(null);
              }}
            >
              {analysisModal?.index != null ? 'Done' : 'Add'}
            </Button>
          </Row>
        }
      />

      <Modal
        headerText={actionModal?.index != null ? 'Edit Action' : 'Add Action'}
        isShowing={!!actionModal}
        hide={() => setActionModal(null)}
        type="custom"
        component={
          <div className="rule-modal-content">
            <Show visible={!!actionModal}>
              <ActionEditor
                action={actionModal?.draft ?? createDefaultAction()}
                descriptors={descriptors}
                phase={actionModal ? definition.steps[actionModal.stepIndex].phase : 'process'}
                analyses={actionModal ? definition.steps[actionModal.stepIndex].analyses : []}
                dedupeRefs={
                  // Unnamed dedupe actions share the 'dedupe' result (latest verdict wins),
                  // so identical refs collapse to one gate option.
                  actionModal
                    ? Array.from(
                        new Set(
                          definition.steps[actionModal.stepIndex].actions
                            .filter((a) => a.type === 'dedupe')
                            .map((a) => `${a.name || 'dedupe'}.isDuplicate`),
                        ),
                      )
                    : []
                }
                objectiveNames={Array.from(
                  new Set(
                    definition.steps
                      .flatMap((s) => s.actions)
                      .filter((a) => a.type === 'score' && a.objective)
                      .map((a) => `${a.objective}`),
                  ),
                ).sort()}
                collectionNames={collectionNames}
                draftNames={
                  actionModal
                    ? draftNamesBefore(definition.steps[actionModal.stepIndex], actionModal.index)
                    : []
                }
                filterOptions={filterOptions}
                reportOptions={reportOptions}
                notificationOptions={notificationOptions}
                actionOptions={actionOptions}
                promptNames={promptNames}
                onChange={(draft) =>
                  setActionModal((state) => (state ? { ...state, draft } : state))
                }
              />
            </Show>
          </div>
        }
        customButtons={
          <Row justifyContent="flex-end" gap="0.5rem" width="100%">
            <Button
              className="header-btn-outline"
              variant={ButtonVariant.secondary}
              onClick={() => setActionModal(null)}
            >
              Cancel
            </Button>
            <Button
              className="header-btn-save"
              onClick={() => {
                if (!actionModal) return;
                if (!actionModal.draft.type) {
                  toast.error('Pick an action.');
                  return;
                }
                const step = definition.steps[actionModal.stepIndex];
                const actions = [...step.actions];
                if (actionModal.index != null) actions[actionModal.index] = actionModal.draft;
                else actions.push(actionModal.draft);
                setStep(actionModal.stepIndex, { ...step, actions });
                setActionModal(null);
              }}
            >
              {actionModal?.index != null ? 'Done' : 'Add'}
            </Button>
          </Row>
        }
      />
    </Col>
  );
};
