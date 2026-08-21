import React from 'react';
import { Col, Row, Show } from 'tno-core';

import { type IAutomationRunSummaryModel, type IAutomationVariantSummaryModel } from './interfaces';

type IAutomationChangeModel = IAutomationVariantSummaryModel['changes'][number];

/** Group changes by the content item they apply to, preserving first-seen order. */
const groupByContent = (
  changes: IAutomationChangeModel[],
): [string, IAutomationChangeModel[]][] => {
  const map = new Map<string, IAutomationChangeModel[]>();
  changes.forEach((change) => {
    const key = change.contentRef || '(run)';
    const list = map.get(key) ?? [];
    list.push(change);
    map.set(key, list);
  });
  return Array.from(map.entries());
};

/** Parse a run's summary JSON when it was produced by the definition engine; null otherwise. */
export const parseRunSummary = (summary?: string | null): IAutomationRunSummaryModel | null => {
  if (!summary) return null;
  try {
    const parsed = JSON.parse(summary);
    return parsed?.engineVersion === 2 ? (parsed as IAutomationRunSummaryModel) : null;
  } catch {
    return null;
  }
};

export interface IRunOutcomeProps {
  summary: IAutomationRunSummaryModel;
}

/**
 * The run outcome: per-step counts and cost, the change set (intended, on a dry run),
 * exclusions with reasons, collection sizes, draft-to-id mapping, and — for comparison runs —
 * the per-item differences between the two variants' intended changes.
 */
export const RunOutcome: React.FC<IRunOutcomeProps> = ({ summary }) => (
  <Col className="automation-run-outcome" gap="0.5rem">
    <Show visible={summary.isDryRun}>
      <p className="automation-dry-run-banner">
        DRY RUN — every decision and change below was computed and logged; nothing was written.
      </p>
    </Show>
    <Show visible={summary.isComparison}>
      <h3>Comparison</h3>
      <p className="automation-field-help">
        Both variants executed dry over the same trigger. Differences list the intended changes only
        one variant produced.
      </p>
      <Show visible={summary.differences.length === 0}>
        <p>The variants intend identical changes.</p>
      </Show>
      <Show visible={summary.differences.length > 0}>
        <table className="automation-table">
          <thead>
            <tr>
              <th>Content</th>
              <th>Only A (current)</th>
              <th>Only B (candidate)</th>
            </tr>
          </thead>
          <tbody>
            {summary.differences.map((difference, index) => (
              <tr key={index}>
                <td>{difference.contentRef}</td>
                <td>{difference.onlyA.join('; ')}</td>
                <td>{difference.onlyB.join('; ')}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </Show>
    </Show>
    <Show visible={!!summary.variantA}>
      <VariantOutcome
        variant={summary.variantA!}
        title={summary.isComparison ? 'Variant A (current definition)' : 'Outcome'}
      />
    </Show>
    <Show visible={!!summary.variantB}>
      <VariantOutcome variant={summary.variantB!} title="Variant B (candidate definition)" />
    </Show>
  </Col>
);

const VariantOutcome: React.FC<{ variant: IAutomationVariantSummaryModel; title: string }> = ({
  variant,
  title,
}) => (
  <Col gap="0.5rem">
    <h3>{title}</h3>
    <Row gap="1rem" className="automation-run-totals" nowrap>
      <span>
        <label>LLM calls:</label> {variant.llmCalls.toLocaleString()}
      </span>
      <span>
        <label>Tokens:</label> {variant.promptTokens.toLocaleString()} prompt /{' '}
        {variant.completionTokens.toLocaleString()} completion
      </span>
      <span>
        <label>Duration:</label> {(variant.durationMs / 1000).toFixed(1)}s
      </span>
      <span>
        <label>Changes:</label> {variant.changes.length.toLocaleString()}
      </span>
      <span>
        <label>Excluded:</label> {variant.excluded.length.toLocaleString()}
      </span>
    </Row>
    <table className="automation-table">
      <thead>
        <tr>
          <th>Step</th>
          <th>Phase</th>
          <th>Items</th>
          <th>Executions</th>
          <th>Skipped</th>
          <th>Excluded</th>
          <th>Aborted</th>
          <th>Failures</th>
          <th>LLM calls</th>
          <th>Duration</th>
        </tr>
      </thead>
      <tbody>
        {variant.steps.map((step, index) => (
          <tr key={index}>
            <td>{step.name}</td>
            <td>{step.phase}</td>
            <td>{step.items}</td>
            <td>{step.executions}</td>
            <td>{step.skipped}</td>
            <td>{step.excluded}</td>
            <td>{step.aborted}</td>
            <td>{step.failures}</td>
            <td>{step.llmCalls}</td>
            <td>{(step.durationMs / 1000).toFixed(1)}s</td>
          </tr>
        ))}
      </tbody>
    </table>
    <Show visible={Object.keys(variant.collections).length > 0}>
      <p className="automation-field-help">
        Collections:{' '}
        {Object.entries(variant.collections)
          .map(([name, count]) => `${name} (${count})`)
          .join(', ')}
      </p>
    </Show>
    <Show visible={Object.keys(variant.draftIds).length > 0}>
      <p className="automation-field-help">
        Created content:{' '}
        {Object.entries(variant.draftIds)
          .map(([tempKey, id]) => `${tempKey} → ${id}`)
          .join(', ')}
      </p>
    </Show>
    <Show visible={variant.flushFailures.length > 0}>
      <details className="automation-findings">
        <summary>Unwritten changes (flush failures) — {variant.flushFailures.length}</summary>
        <ul className="automation-flush-list">
          {variant.flushFailures.map((failure, index) => (
            <li key={index}>{failure}</li>
          ))}
        </ul>
      </details>
    </Show>
    <Show visible={variant.excluded.length > 0}>
      <details>
        <summary>Excluded items ({variant.excluded.length})</summary>
        <table className="automation-table">
          <thead>
            <tr>
              <th>Content</th>
              <th>Step</th>
              <th>Reason</th>
            </tr>
          </thead>
          <tbody>
            {variant.excluded.map((exclusion, index) => (
              <tr key={index}>
                <td>{exclusion.contentRef}</td>
                <td>{exclusion.step}</td>
                <td>{exclusion.reason}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </details>
    </Show>
    <Show visible={variant.changes.length > 0}>
      <details>
        <summary>
          Changes ({variant.changes.length} on{' '}
          {groupByContent(variant.changes.slice(0, 500)).length} item
          {groupByContent(variant.changes.slice(0, 500)).length === 1 ? '' : 's'})
        </summary>
        {groupByContent(variant.changes.slice(0, 500)).map(([contentRef, items]) => (
          <details key={contentRef} className="automation-change-group">
            <summary>
              Content {contentRef} — {items.length} change{items.length === 1 ? '' : 's'}
            </summary>
            <table className="automation-table">
              <thead>
                <tr>
                  <th>Type</th>
                  <th>Field</th>
                  <th>Value</th>
                  <th>Step</th>
                </tr>
              </thead>
              <tbody>
                {items.map((change, index) => (
                  <tr key={index}>
                    <td>{change.type}</td>
                    <td>{change.field ?? ''}</td>
                    <td className="automation-cell-clip">{change.value ?? ''}</td>
                    <td>{change.step ?? ''}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </details>
        ))}
        <Show visible={variant.changes.length > 500}>
          <p className="automation-field-help">
            Showing the first 500 of {variant.changes.length}.
          </p>
        </Show>
      </details>
    </Show>
  </Col>
);
