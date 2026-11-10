import React from 'react';
import { Col, Row, Show } from 'tno-core';

import {
  type IAutomationRunSummaryModel,
  type IAutomationSaveModel,
  type IAutomationScoredItemModel,
  type IAutomationVariantSummaryModel,
} from './interfaces';

type IAutomationChangeModel = IAutomationVariantSummaryModel['changes'][number];

/** The most rows any one outcome table renders; the rest are reported as a count. */
const MAX_ROWS = 500;

/**
 * Run-level change types. Their `contentRef` is NOT a content id — it is the id of the report or
 * notification that was published — so they are listed separately rather than grouped as though
 * they were changes to a story.
 */
const RUN_CHANGE_TYPES: Record<string, 'report' | 'notification'> = {
  'run-report': 'report',
  'run-notification': 'notification',
};

const isRunChange = (change: IAutomationChangeModel) => !!RUN_CHANGE_TYPES[change.type];

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

/**
 * What a change's reference points at. A draft's reference is its temp key rather than an id, so
 * anything non-numeric is labelled as a draft.
 */
const contentRefLabel = (contentRef: string): string => {
  if (!contentRef || contentRef === '(run)') return 'Run';
  return /^\d+$/.test(contentRef) ? `Content ${contentRef}` : `Draft ${contentRef}`;
};

const isContentId = (contentRef?: string | null): boolean => /^\d+$/.test(contentRef ?? '');

/**
 * A content id, linked to its editor page in a new tab. A draft is referenced by its temp key
 * until it is saved and has no page to open, so it renders as plain text.
 */
const ContentRefLink: React.FC<{ contentRef?: string | null }> = ({ contentRef }) =>
  isContentId(contentRef) ? (
    <a
      className="automation-content-link"
      href={`/contents/${contentRef}`}
      target="_blank"
      rel="noopener noreferrer"
      title={`Open content ${contentRef} in a new tab`}
      // Inside a <summary> a click would otherwise toggle the disclosure instead of following.
      onClick={(e) => e.stopPropagation()}
    >
      {contentRef}
    </a>
  ) : (
    <>{contentRef ?? ''}</>
  );

/**
 * Score → number of items, highest score first. The summary stores the distribution as a JSON
 * object, so its keys arrive as strings.
 */
const distributionRows = (distribution?: Record<string, number> | null): [number, number][] =>
  Object.entries(distribution ?? {})
    .map(([score, count]) => [Number(score), count] as [number, number])
    .sort((a, b) => b[0] - a[0]);

/** How many items carried each score — the whole point of a run that ranks stories. */
const ScoreDistribution: React.FC<{ distribution?: Record<string, number> | null }> = ({
  distribution,
}) => {
  const rows = distributionRows(distribution);
  const total = rows.reduce((sum, [, count]) => sum + count, 0);
  return (
    <Show visible={rows.length > 0}>
      <table className="automation-table automation-distribution-table">
        <thead>
          <tr>
            <th>Score</th>
            <th>Items</th>
            <th>Share</th>
          </tr>
        </thead>
        <tbody>
          {rows.map(([score, count]) => (
            <tr key={score}>
              <td>{score}</td>
              <td>{count.toLocaleString()}</td>
              <td>{total > 0 ? `${Math.round((count / total) * 100)}%` : ''}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </Show>
  );
};

/** A list of scored items; the rank column is shown for a selection's ordered result. */
const ScoredItemTable: React.FC<{ items: IAutomationScoredItemModel[]; showRank?: boolean }> = ({
  items,
  showRank = false,
}) => (
  <>
    <table className="automation-table">
      <thead>
        <tr>
          <Show visible={showRank}>
            <th>Rank</th>
          </Show>
          <th>Score</th>
          <th>Content</th>
          <th>Headline</th>
          <th>Step</th>
        </tr>
      </thead>
      <tbody>
        {items.slice(0, MAX_ROWS).map((item, index) => (
          <tr key={`${item.contentRef}-${index}`}>
            <Show visible={showRank}>
              <td>{index + 1}</td>
            </Show>
            <td>{item.score}</td>
            <td>
              <ContentRefLink contentRef={item.contentRef} />
            </td>
            <td className="automation-cell-clip">{item.headline ?? ''}</td>
            <td>{item.step ?? ''}</td>
          </tr>
        ))}
      </tbody>
    </table>
    <Show visible={items.length > MAX_ROWS}>
      <p className="automation-field-help">
        Showing the first {MAX_ROWS} of {items.length.toLocaleString()}.
      </p>
    </Show>
  </>
);

/** How many items each field was written on, so a save reads as 'what changed', not 'how many'. */
const fieldTally = (saves: IAutomationSaveModel[]): [string, number][] => {
  const tally = new Map<string, number>();
  saves.forEach((save) =>
    (save.fields ?? []).forEach((field) => tally.set(field, (tally.get(field) ?? 0) + 1)),
  );
  return Array.from(tally.entries()).sort((a, b) => b[1] - a[1] || a[0].localeCompare(b[0]));
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
  /** Report id -> name, so a published report reads as a name instead of a bare id. */
  reportNames?: Record<string, string>;
  /** Notification id -> name. */
  notificationNames?: Record<string, string>;
}

/**
 * The run outcome: per-step counts and cost, the change set (intended, on a dry run),
 * exclusions with reasons, collection sizes, draft-to-id mapping, and — for comparison runs —
 * the per-item differences between the two variants' intended changes.
 */
export const RunOutcome: React.FC<IRunOutcomeProps> = ({
  summary,
  reportNames,
  notificationNames,
}) => (
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
                <td>
                  <ContentRefLink contentRef={difference.contentRef} />
                </td>
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
        isDryRun={summary.isDryRun}
        reportNames={reportNames}
        notificationNames={notificationNames}
        title={summary.isComparison ? 'Variant A (current definition)' : 'Outcome'}
      />
    </Show>
    <Show visible={!!summary.variantB}>
      <VariantOutcome
        variant={summary.variantB!}
        isDryRun={summary.isDryRun}
        reportNames={reportNames}
        notificationNames={notificationNames}
        title="Variant B (candidate definition)"
      />
    </Show>
  </Col>
);

const VariantOutcome: React.FC<{
  variant: IAutomationVariantSummaryModel;
  title: string;
  isDryRun: boolean;
  reportNames?: Record<string, string>;
  notificationNames?: Record<string, string>;
}> = ({ variant, title, isDryRun, reportNames, notificationNames }) => {
  // Runs recorded before these sections existed carry no scores/selections/saves — default them
  // so an older run still opens.
  const scores = variant.scores ?? [];
  const selections = variant.selections ?? [];
  const saves = variant.saves ?? [];
  const scoredCount = scores.reduce((sum, objective) => sum + objective.items.length, 0);
  const selectedCount = selections.reduce((sum, selection) => sum + selection.selected.length, 0);
  const savedFields = fieldTally(saves);
  // Reports and notifications are run-level: their reference is the report/notification id, not a
  // content id, so they never belong in the per-item grouping.
  const runChanges = variant.changes.filter(isRunChange);
  const itemChanges = variant.changes.filter((change) => !isRunChange(change));
  const runChangeName = (change: IAutomationChangeModel) => {
    const kind = RUN_CHANGE_TYPES[change.type];
    const name = (kind === 'report' ? reportNames : notificationNames)?.[change.contentRef];
    return name ? `${name} (#${change.contentRef})` : `#${change.contentRef}`;
  };
  return (
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
        <Show visible={scoredCount > 0}>
          <span>
            <label>Scored:</label> {scoredCount.toLocaleString()}
          </span>
        </Show>
        <Show visible={selections.length > 0}>
          <span>
            <label>Selected:</label> {selectedCount.toLocaleString()}
          </span>
        </Show>
        <Show visible={saves.length > 0}>
          <span>
            <label>Saved:</label> {saves.length.toLocaleString()}
          </span>
        </Show>
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
      <Show visible={scores.length > 0}>
        <h4>Scoring</h4>
        {scores.map((objective) => (
          <details key={objective.objective} className="automation-change-group">
            <summary>
              Score Content — &lsquo;{objective.objective}&rsquo; ({objective.items.length} item
              {objective.items.length === 1 ? '' : 's'} scored
              {objective.unscored > 0 ? `, ${objective.unscored} not scored` : ''})
            </summary>
            <p className="automation-field-help">
              Recorded by {objective.steps.length === 1 ? 'step' : 'steps'}{' '}
              {objective.steps.join(', ')}.
              {objective.unscored > 0
                ? ` ${objective.unscored} item(s) returned a value that was not a number and were never scored.`
                : ''}
            </p>
            <ScoreDistribution distribution={objective.distribution} />
            <ScoredItemTable items={objective.items} />
          </details>
        ))}
      </Show>
      <Show visible={selections.length > 0}>
        <h4>Top selections</h4>
        {selections.map((selection, index) => (
          <details key={`${selection.objective}-${index}`} className="automation-change-group">
            <summary>
              Select Top Scored — &lsquo;{selection.objective}&rsquo; (kept{' '}
              {selection.selected.length} of {selection.candidates.toLocaleString()} scored item
              {selection.candidates === 1 ? '' : 's'})
            </summary>
            <p className="automation-field-help">
              Kept {selection.rule ?? `the top ${selection.take}`}; ranked by {selection.sortedBy}
              {selection.minScore != null
                ? `; ${(
                    selection.qualified ?? selection.selected.length
                  ).toLocaleString()} of ${selection.candidates.toLocaleString()} scored item(s) met the ${
                    selection.minScore
                  } threshold`
                : ''}
              {selection.into ? `; selected items added to ${selection.into}` : ''}
              {selection.contentAction ? `; stamped '${selection.contentAction}'` : ''}
              {selection.step ? `; step ${selection.step}` : ''}.
            </p>
            <Show visible={selection.unresolved.length > 0}>
              <p className="automation-field-help">
                {selection.unresolved.length} ranked item(s) no longer resolved and were dropped:{' '}
                {selection.unresolved.join(', ')}.
              </p>
            </Show>
            <ScoreDistribution distribution={selection.distribution} />
            <ScoredItemTable items={selection.selected} showRank />
          </details>
        ))}
      </Show>
      <Show visible={saves.length > 0}>
        <h4>{isDryRun ? 'Content that would be saved' : 'Saved content'}</h4>
        <p className="automation-field-help">
          {saves.length.toLocaleString()} item{saves.length === 1 ? '' : 's'}{' '}
          {isDryRun ? 'would be written' : 'written'}
          {savedFields.length > 0
            ? ` — fields: ${savedFields.map(([field, count]) => `${field} (${count})`).join(', ')}`
            : ''}
          .
        </p>
        <details className="automation-change-group">
          <summary>
            {isDryRun ? 'Items that would be saved' : 'Saved items'} (
            {saves.length.toLocaleString()})
          </summary>
          <table className="automation-table">
            <thead>
              <tr>
                <th>Content</th>
                <th>Headline</th>
                <th>Fields written</th>
                <th>From</th>
                <th>Result</th>
                <th>Step</th>
              </tr>
            </thead>
            <tbody>
              {saves.slice(0, MAX_ROWS).map((save, index) => (
                <tr key={`${save.contentRef}-${index}`}>
                  <td>
                    <ContentRefLink contentRef={save.contentRef} />
                  </td>
                  <td className="automation-cell-clip">{save.headline ?? ''}</td>
                  <td>{save.fields.length > 0 ? save.fields.join(', ') : '—'}</td>
                  <td>{save.collection ?? save.action ?? ''}</td>
                  <td>
                    {save.outcome}
                    {save.indexed ? ' (indexed)' : ''}
                    {save.error ? ` — ${save.error}` : ''}
                  </td>
                  <td>{save.step ?? ''}</td>
                </tr>
              ))}
            </tbody>
          </table>
          <Show visible={saves.length > MAX_ROWS}>
            <p className="automation-field-help">
              Showing the first {MAX_ROWS} of {saves.length.toLocaleString()}.
            </p>
          </Show>
        </details>
      </Show>
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
          {Object.entries(variant.draftIds).map(([tempKey, id], index) => (
            <React.Fragment key={tempKey}>
              {index > 0 ? ', ' : ''}
              {tempKey} → <ContentRefLink contentRef={`${id}`} />
            </React.Fragment>
          ))}
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
                  <td>
                    <ContentRefLink contentRef={exclusion.contentRef} />
                  </td>
                  <td>{exclusion.step}</td>
                  <td>{exclusion.reason}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </details>
      </Show>
      <Show visible={runChanges.length > 0}>
        <h4>
          {isDryRun ? 'Reports and notifications that would run' : 'Reports and notifications'}
        </h4>
        <table className="automation-table">
          <thead>
            <tr>
              <th>Type</th>
              <th>Name</th>
              <th>Using collection</th>
              <th>Step</th>
            </tr>
          </thead>
          <tbody>
            {runChanges.map((change, index) => (
              <tr key={index}>
                <td>{RUN_CHANGE_TYPES[change.type] === 'report' ? 'Report' : 'Notification'}</td>
                <td>{runChangeName(change)}</td>
                <td>{change.value ?? ''}</td>
                <td>{change.step ?? ''}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </Show>
      <Show visible={itemChanges.length > 0}>
        <details>
          <summary>
            Content changes ({itemChanges.length} on{' '}
            {groupByContent(itemChanges.slice(0, MAX_ROWS)).length} item
            {groupByContent(itemChanges.slice(0, MAX_ROWS)).length === 1 ? '' : 's'})
          </summary>
          {groupByContent(itemChanges.slice(0, MAX_ROWS)).map(([contentRef, items]) => (
            <details key={contentRef} className="automation-change-group">
              <summary>
                {isContentId(contentRef) ? (
                  <>
                    Content <ContentRefLink contentRef={contentRef} />
                  </>
                ) : (
                  contentRefLabel(contentRef)
                )}{' '}
                — {items.length} change{items.length === 1 ? '' : 's'}
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
          <Show visible={itemChanges.length > MAX_ROWS}>
            <p className="automation-field-help">
              Showing the first {MAX_ROWS} of {itemChanges.length}.
            </p>
          </Show>
        </details>
      </Show>
    </Col>
  );
};
