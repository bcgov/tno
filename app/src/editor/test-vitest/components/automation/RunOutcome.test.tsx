import { render } from '@testing-library/react';
import {
  type IAutomationRunSummaryModel,
  type IAutomationVariantSummaryModel,
  parseRunSummary,
  RunOutcome,
} from 'features/admin/automation/designer';
import React from 'react';
import { TestWrapper } from 'test/utils';

/** A variant carrying only the members every engine-v2 run has ever written. */
const legacyVariant = (): IAutomationVariantSummaryModel => ({
  steps: [
    {
      name: 'score-stories',
      phase: 'process',
      items: 3,
      executions: 3,
      skipped: 0,
      excluded: 0,
      aborted: 0,
      failures: 0,
      llmCalls: 3,
      durationMs: 1200,
    },
  ],
  changes: [],
  collections: {},
  excluded: [],
  draftIds: {},
  llmCalls: 3,
  promptTokens: 100,
  completionTokens: 20,
  durationMs: 1500,
  flushFailures: [],
});

const summaryOf = (variant: IAutomationVariantSummaryModel): IAutomationRunSummaryModel => ({
  engineVersion: 2,
  isDryRun: false,
  isComparison: false,
  variantA: variant,
  differences: [],
});

describe('parseRunSummary', () => {
  it('returns null for nothing, malformed JSON, and a pre-v2 summary', () => {
    expect(parseRunSummary(undefined)).toBeNull();
    expect(parseRunSummary('')).toBeNull();
    expect(parseRunSummary('{ not json')).toBeNull();
    expect(parseRunSummary(JSON.stringify({ engineVersion: 1 }))).toBeNull();
  });

  it('returns the summary for an engine-v2 payload', () => {
    const parsed = parseRunSummary(JSON.stringify(summaryOf(legacyVariant())));
    expect(parsed?.engineVersion).toEqual(2);
    expect(parsed?.variantA?.steps).toHaveLength(1);
  });
});

describe('RunOutcome', () => {
  /**
   * The regression that matters: runs recorded before scoring/selection/save reporting existed
   * carry none of those members, and their Outcome tab must still render.
   */
  it('renders a run whose summary predates the scoring, selection and save sections', () => {
    const { container, queryByText } = render(
      <TestWrapper>
        <RunOutcome summary={summaryOf(legacyVariant())} />
      </TestWrapper>,
    );
    expect(container.querySelector('.automation-run-outcome')).not.toBeNull();
    expect(queryByText('Scoring')).toBeNull();
    expect(queryByText('Top selections')).toBeNull();
    expect(queryByText('Saved content')).toBeNull();
  });

  it('reports which items were scored, how many carried each score, and what was selected', () => {
    const variant: IAutomationVariantSummaryModel = {
      ...legacyVariant(),
      scores: [
        {
          objective: 'top-story',
          steps: ['score-stories'],
          items: [
            { contentRef: '9', score: 9, headline: 'Budget tabled', step: 'score-stories' },
            { contentRef: '10', score: 7, headline: 'Ferry delays', step: 'score-stories' },
            { contentRef: '11', score: 7, headline: 'Snow warning', step: 'score-stories' },
          ],
          // Deliberately out of order: JSON object keys arrive ascending, the view sorts.
          distribution: { '7': 2, '9': 1 },
          unscored: 1,
        },
      ],
      selections: [
        {
          objective: 'top-story',
          step: 'pick-top',
          action: 'select-top',
          sortedBy: 'score descending, then content id ascending',
          take: 2,
          candidates: 3,
          into: 'top',
          contentAction: 'Top Story',
          selected: [
            { contentRef: '9', score: 9, headline: 'Budget tabled', step: 'pick-top' },
            { contentRef: '10', score: 7, headline: 'Ferry delays', step: 'pick-top' },
          ],
          distribution: { '7': 2, '9': 1 },
          unresolved: [],
        },
      ],
    };
    const { container, getByText, getAllByText } = render(
      <TestWrapper>
        <RunOutcome summary={summaryOf(variant)} />
      </TestWrapper>,
    );
    getByText('Scoring');
    getByText('Top selections');
    // The headline appears in both the objective's table and the selection's table.
    expect(getAllByText('Budget tabled')).toHaveLength(2);
    // Both the objective's and the selection's distributions render, highest score first.
    const distributions = container.querySelectorAll('.automation-distribution-table');
    expect(distributions).toHaveLength(2);
    const firstScores = Array.from(
      distributions[0].querySelectorAll('tbody tr td:first-child'),
    ).map((cell) => cell.textContent);
    expect(firstScores).toEqual(['9', '7']);
    expect(getAllByText(/score descending, then content id ascending/).length).toBeGreaterThan(0);
  });

  it('links content ids to their editor page in a new tab, and never links a draft', () => {
    const variant: IAutomationVariantSummaryModel = {
      ...legacyVariant(),
      changes: [
        { type: 'update-field', contentRef: '42', field: 'headline', value: 'x', step: 'edit' },
        { type: 'create-content', contentRef: 'digest-1', step: 'build' },
      ],
    };
    const { container } = render(
      <TestWrapper>
        <RunOutcome summary={summaryOf(variant)} />
      </TestWrapper>,
    );
    const link = container.querySelector('a.automation-content-link') as HTMLAnchorElement;
    expect(link).not.toBeNull();
    expect(link.getAttribute('href')).toEqual('/contents/42');
    expect(link.getAttribute('target')).toEqual('_blank');
    expect(link.getAttribute('rel')).toContain('noopener');
    // The draft's temp key is not an id and has no page to open.
    expect(container.querySelectorAll('a.automation-content-link')).toHaveLength(1);
  });

  it('lists published reports and notifications apart from the content changes', () => {
    const variant: IAutomationVariantSummaryModel = {
      ...legacyVariant(),
      changes: [
        { type: 'run-report', contentRef: '5', step: 'reporting' },
        { type: 'run-notification', contentRef: '4', step: 'reporting' },
      ],
    };
    const { container, getByText, queryByText } = render(
      <TestWrapper>
        <RunOutcome
          summary={summaryOf(variant)}
          reportNames={{ '5': 'Daily Front Page' }}
          notificationNames={{ '4': 'Breaking Alert' }}
        />
      </TestWrapper>,
    );
    getByText('Reports and notifications');
    getByText('Daily Front Page (#5)');
    getByText('Breaking Alert (#4)');
    // A report id is not a content id: it must never be labelled or linked as one.
    expect(queryByText('Content 5')).toBeNull();
    expect(container.querySelector('a.automation-content-link')).toBeNull();
  });

  it('names the fields each save wrote', () => {
    const variant: IAutomationVariantSummaryModel = {
      ...legacyVariant(),
      saves: [
        {
          contentRef: '9',
          step: 'save',
          action: 'save-collection',
          collection: 'top',
          headline: 'Budget tabled',
          fields: ['headline', 'tags (BUD)'],
          outcome: 'saved',
          indexed: true,
        },
        {
          contentRef: '10',
          step: 'save',
          action: 'save-collection',
          collection: 'top',
          headline: 'Ferry delays',
          fields: ['headline'],
          outcome: 'saved',
          indexed: true,
        },
      ],
    };
    const { getByText } = render(
      <TestWrapper>
        <RunOutcome summary={summaryOf(variant)} />
      </TestWrapper>,
    );
    getByText('Saved content');
    // The tally names the fields and how many items carried each.
    getByText(/headline \(2\)/);
    getByText(/tags \(BUD\) \(1\)/);
  });
});
