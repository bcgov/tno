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
import { FaCircleInfo } from 'react-icons/fa6';
import { toast } from 'react-toastify';
import { Button, ButtonVariant, Col, type IOptionItem, Modal, Row, Show, TextArea } from 'tno-core';

import { StrictModeDroppable } from '../StrictModeDroppable';
import { findOptionByValue } from '../utils';
import {
  collectV2CollectionNames,
  createDefaultV2Action,
  createDefaultV2Step,
  parseV2Definition,
  serializeV2Definition,
} from './constants';
import {
  type IV2Action,
  type IV2ActionDescriptor,
  type IV2Analysis,
  type IV2Definition,
  type IV2Step,
  type IV2ValidationError,
} from './interfaces';
import { V2ActionEditor } from './V2ActionEditor';
import { V2AnalysisEditor } from './V2AnalysisEditor';
import { V2PromptLibrary } from './V2PromptLibrary';
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

interface IStepModalState {
  /** The step index being edited, or null when adding. */
  index: number | null;
  draft: IV2Step;
}

interface IActionModalState {
  stepIndex: number;
  /** The action index being edited, or null when adding. */
  index: number | null;
  draft: IV2Action;
}

interface IAnalysisModalState {
  stepIndex: number;
  /** The analysis index being edited, or null when adding. */
  index: number | null;
  draft: IV2Analysis;
}

const deepCopy = <T,>(value: T): T => JSON.parse(JSON.stringify(value)) as T;

/**
 * The v2 profile designer: the prompt library, a steps grid (name, phase, source, counts - rows
 * drag to reorder, chevrons expand to the step's analyses and actions, pencils open modal forms),
 * and on-demand validation with per-path findings. A raw JSON editor is available for power
 * edits; the same document round-trips both ways.
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
  const [expanded, setExpanded] = React.useState<Set<number>>(new Set());
  const [showJson, setShowJson] = React.useState(false);
  const [jsonDraft, setJsonDraft] = React.useState('');
  const [findings, setFindings] = React.useState<IV2ValidationError[] | null>(null);
  const [stepModal, setStepModal] = React.useState<IStepModalState | null>(null);
  const [actionModal, setActionModal] = React.useState<IActionModalState | null>(null);
  const [analysisModal, setAnalysisModal] = React.useState<IAnalysisModalState | null>(null);

  const update = (next: IV2Definition) => onChange(serializeV2Definition(next));
  const promptNames = Object.keys(definition.prompts);
  const collectionNames = collectV2CollectionNames(definition);

  const setStep = (index: number, step: IV2Step) => {
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

    if (source.droppableId === 'v2-steps') {
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

    if (source.droppableId.startsWith('v2-actions-')) {
      const stepIndex = Number(source.droppableId.replace('v2-actions-', ''));
      const step = definition.steps[stepIndex];
      if (!step) return;
      const actions = [...step.actions];
      const [moved] = actions.splice(source.index, 1);
      actions.splice(destination.index, 0, moved);
      setStep(stepIndex, { ...step, actions });
    }
  };

  const sourceLabel = (step: IV2Step): React.ReactNode => {
    if (step.phase === 'init' || !step.source) return '—';
    const source = step.source;
    if (source?.from === 'collection')
      return <span className="v2-prompt-name">{source.collection ?? '?'}</span>;
    if (source?.from === 'filter')
      return findOptionByValue(filterOptions, source.filter)?.label ?? `filter ${source.filter}`;
    // Legacy documents may still carry an unsupported source kind; validation flags it.
    return source?.from ? `${source.from} (unsupported)` : '—';
  };

  const typeLabel = (type: string): string =>
    descriptors.find((descriptor) => descriptor.type === type)?.label ?? type;

  const runsWhenLabel = (action: IV2Action): React.ReactNode => {
    if (action.confirm || action.when?.from) return 'LLM analysis';
    if (action.when) return 'Condition';
    return <span className="v2-muted">Always run</span>;
  };

  const duplicateStep = (index: number) => {
    const steps = [...definition.steps];
    const copy = deepCopy(steps[index]);
    copy.name = `${copy.name} (copy)`;
    steps.splice(index + 1, 0, copy);
    update({ ...definition, steps });
  };

  /** Drafts declared by content.create actions before the given index (modal draft pickers). */
  const draftNamesBefore = (step: IV2Step, index: number | null): string[] =>
    step.actions
      .slice(0, index ?? step.actions.length)
      .filter((action) => action.type === 'content.create' && !!action.as)
      .map((action) => action.as!);

  return (
    <Col className="v2-designer" gap="0.5rem">
      <Row className="section-header" nowrap>
        <h2>Prompt Library</h2>
      </Row>
      <V2PromptLibrary definition={definition} onChange={update} />

      <Row className="section-header" nowrap>
        <h2>Steps</h2>
        <span
          className="v2-info"
          title="Steps run in phase order (init → process → complete) and in row order within a phase. Every action in a step applies to the item the step iterates; to act on different content, iterate a different collection."
        >
          <FaCircleInfo />
        </span>
      </Row>
      <p className="section-help-text">
        Init steps run once before iteration; each process step iterates its declared source;
        complete steps run once after. Drag rows to reorder within a phase.
        <br />
        <span className="help-accent">
          An analysis executes at most once per item, and only when a consuming action is reachable
          — actions gated by a property condition send no prompt.
        </span>
      </p>
      <div className="v2-grid">
        <Row className="v2-grid-header" nowrap>
          <span className="v2-gc-drag" />
          <span className="v2-gc-collapse">
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
          <span className="v2-gc-name">Step Name</span>
          <span className="v2-gc-phase">Phase</span>
          <span className="v2-gc-source">Source</span>
          <span className="v2-gc-count">Actions</span>
          <span className="v2-gc-save">Save Mode</span>
          <span className="v2-gc-enabled">Enabled</span>
          <span className="v2-gc-actions">
            <button
              type="button"
              className="rule-icon-button"
              aria-label="Add a step"
              title="Add a step"
              onClick={() => setStepModal({ index: null, draft: createDefaultV2Step('process') })}
            >
              <FaPlus />
            </button>
          </span>
        </Row>
        <DragDropContextAny onDragEnd={onDragEnd}>
          <DroppableAny droppableId="v2-steps" type="v2-steps">
            {(provided: any) => (
              <div ref={provided.innerRef} {...provided.droppableProps}>
                {definition.steps.map((step, index) => (
                  <DraggableAny key={`step-${index}`} draggableId={`step-${index}`} index={index}>
                    {(dragProvided: any, dragSnapshot: any) => (
                      <div
                        className={`v2-grid-item${dragSnapshot.isDragging ? ' is-dragging' : ''}`}
                        ref={dragProvided.innerRef}
                        {...dragProvided.draggableProps}
                      >
                        <Row className="v2-grid-row" nowrap>
                          <span
                            className="v2-gc-drag v2-drag-handle"
                            title="Drag to reorder"
                            {...dragProvided.dragHandleProps}
                          >
                            <FaGripLines />
                          </span>
                          <span className="v2-gc-collapse">
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
                          <span className="v2-gc-name">{step.name || '(unnamed step)'}</span>
                          <span className="v2-gc-phase">
                            <span className={`v2-badge v2-phase-${step.phase}`}>{step.phase}</span>
                          </span>
                          <span className="v2-gc-source">{sourceLabel(step)}</span>
                          <span className="v2-gc-count">{step.actions.length}</span>
                          <span className="v2-gc-save">
                            {step.phase !== 'init' ? step.saveMode ?? definition.saveMode : '—'}
                          </span>
                          <span className="v2-gc-enabled">{step.isEnabled ? 'Yes' : 'No'}</span>
                          <span className="v2-gc-actions">
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
                          <div className="v2-grid-expanded">
                            <Show visible={step.phase !== 'init'}>
                              <div className="v2-grid v2-subgrid">
                                <Row className="v2-grid-header" nowrap>
                                  <span className="v2-gc-drag" />
                                  <span className="v2-gc-name">Analysis</span>
                                  <span className="v2-gc-source">Prompt</span>
                                  <span className="v2-gc-sm">Chain</span>
                                  <span className="v2-gc-sm">LLM Override</span>
                                  <span className="v2-gc-sm">Returns</span>
                                  <span className="v2-gc-actions">
                                    <button
                                      type="button"
                                      className="rule-icon-button"
                                      aria-label="Add an analysis"
                                      title="Add an analysis"
                                      onClick={() =>
                                        setAnalysisModal({
                                          stepIndex: index,
                                          index: null,
                                          draft: { name: '', prompt: { text: '' }, returns: {} },
                                        })
                                      }
                                    >
                                      <FaPlus />
                                    </button>
                                  </span>
                                </Row>
                                <Show visible={step.analyses.length === 0}>
                                  <Row className="v2-grid-row" nowrap>
                                    <span className="v2-muted">
                                      No analyses — actions gated by property conditions run without
                                      any LLM call.
                                    </span>
                                  </Row>
                                </Show>
                                {step.analyses.map((analysis, analysisIndex) => (
                                  <Row
                                    key={analysisIndex}
                                    className={`v2-grid-row${
                                      analysisIndex % 2 ? ' v2-row-even' : ''
                                    }`}
                                    nowrap
                                  >
                                    <span className="v2-gc-drag v2-gc-icon">
                                      <FaFlask />
                                    </span>
                                    <span className="v2-gc-name">{analysis.name}</span>
                                    <span className="v2-gc-source">
                                      {analysis.prompt?.ref ? (
                                        <>
                                          <span className="v2-prompt-name">
                                            {analysis.prompt.ref}
                                          </span>
                                          {analysis.prompt.override ? (
                                            <span className="v2-override-note"> + override</span>
                                          ) : null}
                                        </>
                                      ) : (
                                        <span className="v2-muted">(inline text)</span>
                                      )}
                                    </span>
                                    <span className="v2-gc-sm">
                                      {analysis.chain ?? <span className="v2-muted">—</span>}
                                    </span>
                                    <span className="v2-gc-sm">
                                      {findOptionByValue(llmOptions, analysis.llmId)?.label ?? (
                                        <span className="v2-muted">step default</span>
                                      )}
                                    </span>
                                    <span className="v2-gc-sm">
                                      {analysis.raw ? 'Raw response' : 'Configured fields'}
                                    </span>
                                    <span className="v2-gc-actions">
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

                            <div className="v2-grid v2-subgrid">
                              <Row className="v2-grid-header" nowrap>
                                <span className="v2-gc-drag" />
                                <span className="v2-gc-name">Name</span>
                                <span className="v2-gc-source">Action Type</span>
                                <span className="v2-gc-sm">Runs When</span>
                                <span className="v2-gc-name">Filter</span>
                                <span className="v2-gc-name">Into Collection</span>
                                <span className="v2-gc-actions">
                                  <button
                                    type="button"
                                    className="rule-icon-button"
                                    aria-label="Add an action"
                                    title="Add an action"
                                    onClick={() =>
                                      setActionModal({
                                        stepIndex: index,
                                        index: null,
                                        draft: createDefaultV2Action(''),
                                      })
                                    }
                                  >
                                    <FaPlus />
                                  </button>
                                </span>
                              </Row>
                              <DroppableAny
                                droppableId={`v2-actions-${index}`}
                                type={`v2-actions-${index}`}
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
                                              actionSnapshot.isDragging ? 'is-dragging' : undefined
                                            }
                                            ref={actionDrag.innerRef}
                                            {...actionDrag.draggableProps}
                                          >
                                            <Row
                                              className={`v2-grid-row${
                                                actionIndex % 2 ? ' v2-row-even' : ''
                                              }`}
                                              nowrap
                                            >
                                              <span
                                                className="v2-gc-drag v2-drag-handle"
                                                title="Drag to reorder"
                                                {...actionDrag.dragHandleProps}
                                              >
                                                <FaGripLines />
                                              </span>
                                              <span className="v2-gc-name">
                                                {action.name ?? typeLabel(action.type)}
                                              </span>
                                              <span className="v2-gc-source">
                                                <span className="v2-prompt-name">
                                                  {action.type}
                                                </span>
                                              </span>
                                              <span className="v2-gc-sm">
                                                {runsWhenLabel(action)}
                                              </span>
                                              <span className="v2-gc-name">
                                                {action.filter != null ? (
                                                  findOptionByValue(filterOptions, action.filter)
                                                    ?.label ?? `filter ${action.filter}`
                                                ) : (
                                                  <span className="v2-muted">—</span>
                                                )}
                                              </span>
                                              <span className="v2-gc-name">
                                                {action.into ? (
                                                  <span className="v2-prompt-name">
                                                    {action.into}
                                                  </span>
                                                ) : (
                                                  <span className="v2-muted">—</span>
                                                )}
                                              </span>
                                              <span className="v2-gc-actions">
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
          <Row className="v2-grid-row v2-library-empty" nowrap>
            <span>No steps configured.</span>
          </Row>
        </Show>
      </div>

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

      <Modal
        headerText={stepModal?.index != null ? 'Edit Step' : 'Add Step'}
        isShowing={!!stepModal}
        hide={() => setStepModal(null)}
        type="custom"
        component={
          <div className="rule-modal-content">
            <Show visible={!!stepModal}>
              <V2StepEditor
                step={stepModal?.draft ?? createDefaultV2Step()}
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
                // Init steps never declare a source; process and complete steps require one.
                const source = draft.source;
                const sourceIsSet =
                  !!source && (source.from === 'filter' ? !!source.filter : !!source.collection);
                if (draft.phase === 'init') draft.source = undefined;
                else if (!sourceIsSet) {
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
              {stepModal?.index != null ? 'Save' : 'Add'}
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
              <V2AnalysisEditor
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
              {analysisModal?.index != null ? 'Save' : 'Add'}
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
              <V2ActionEditor
                action={actionModal?.draft ?? createDefaultV2Action()}
                descriptors={descriptors}
                phase={actionModal ? definition.steps[actionModal.stepIndex].phase : 'process'}
                analysisNames={
                  actionModal
                    ? definition.steps[actionModal.stepIndex].analyses
                        .map((analysis) => analysis.name)
                        .filter((name) => !!name)
                    : []
                }
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
              {actionModal?.index != null ? 'Save' : 'Add'}
            </Button>
          </Row>
        }
      />
    </Col>
  );
};
