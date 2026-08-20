import React from 'react';
import { FaAngleLeft, FaAngleRight, FaAnglesLeft, FaAnglesRight } from 'react-icons/fa6';
import { Button, ButtonVariant, Col, type IOptionItem, Row, Select, Show, Text } from 'tno-core';

import { findOptionByValue } from '../utils';
import { v2OutcomeBadgeClass, v2RunLogOutcomeOptions } from './constants';
import { ExplainPanel } from './ExplainPanel';
import {
  type IAutomationExplainRequestModel,
  type IAutomationExplainResultModel,
  type IAutomationRunLogFilter,
  type IAutomationRunLogModel,
  type IAutomationRunLogPage,
} from './interfaces';

export interface IRunLogViewerProps {
  /** Poll the current page every few seconds (a run in progress). */
  live?: boolean;
  runId: number;
  onFetch: (runId: number, filter: IAutomationRunLogFilter) => Promise<IAutomationRunLogPage>;
  onExplain: (
    runId: number,
    logId: number,
    request: IAutomationExplainRequestModel,
  ) => Promise<IAutomationExplainResultModel>;
  promptNames?: string[];
  onApplyPrompt?: (name: string, text: string) => void;
}

const PAGE_SIZE = 50;

/**
 * The run's decision log: every prompt, response, and engine decision in execution order,
 * filterable and searchable, paged so a full day of entries never loads at once. Non-LLM entries
 * (condition gates, exclusions, flushes) render distinctly — they carry no token cost. Any entry
 * opens an explain-and-improve conversation.
 */
export const RunLogViewer: React.FC<IRunLogViewerProps> = ({
  runId,
  onFetch,
  onExplain,
  promptNames,
  onApplyPrompt,
  live = false,
}) => {
  const [page, setPage] = React.useState(1);
  const [step, setStep] = React.useState('');
  const [action, setAction] = React.useState('');
  const [outcome, setOutcome] = React.useState('');
  const [contentId, setContentId] = React.useState('');
  const [search, setSearch] = React.useState('');
  const [applied, setApplied] = React.useState(0);
  const [data, setData] = React.useState<IAutomationRunLogPage | null>(null);
  const [loading, setLoading] = React.useState(false);
  const [expanded, setExpanded] = React.useState<Set<number>>(new Set());
  const [explaining, setExplaining] = React.useState<number | null>(null);
  const [tick, setTick] = React.useState(0);

  // Live tail: while the run is executing, re-fetch the current page every few seconds so the
  // log streams in without a manual refresh.
  React.useEffect(() => {
    if (!live) return;
    const timer = window.setInterval(() => setTick((value) => value + 1), 5000);
    return () => window.clearInterval(timer);
  }, [live]);

  React.useEffect(() => {
    let cancelled = false;
    setLoading(true);
    onFetch(runId, {
      step: step || undefined,
      action: action || undefined,
      outcome: outcome || undefined,
      contentId: contentId ? Number(contentId) : undefined,
      search: search || undefined,
      page,
      qty: PAGE_SIZE,
    })
      .then((result) => {
        if (!cancelled) setData(result);
      })
      .catch(() => {})
      .finally(() => {
        if (!cancelled) setLoading(false);
      });
    return () => {
      cancelled = true;
    };
    // Refetch on paging and on explicit 'Apply filters' (the applied counter).
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [runId, page, applied, tick]);

  const totalPages = data ? Math.max(1, Math.ceil(data.total / PAGE_SIZE)) : 1;

  const exportLog = () => {
    const blob = new Blob([JSON.stringify(data?.items ?? [], undefined, 2)], {
      type: 'application/json',
    });
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = `automation-run-${runId}-log-page-${page}.json`;
    link.click();
    URL.revokeObjectURL(url);
  };

  return (
    <Col className="v2-log-viewer" gap="0.5rem">
      <Row gap="0.5rem" alignItems="flex-end" nowrap>
        <Text
          name="log-step"
          label="Step"
          width="10rem"
          value={step}
          onChange={(e) => setStep(e.target.value)}
        />
        <Text
          name="log-action"
          label="Action/analysis"
          width="10rem"
          value={action}
          onChange={(e) => setAction(e.target.value)}
        />
        <Select
          name="log-outcome"
          label="Outcome"
          width="12rem"
          isClearable={false}
          options={v2RunLogOutcomeOptions}
          value={findOptionByValue(v2RunLogOutcomeOptions, outcome)}
          onChange={(newValue) => {
            const option = newValue as IOptionItem;
            setOutcome(option?.value ? `${option.value}` : '');
          }}
        />
        <Text
          name="log-content-id"
          label="Content id"
          width="8rem"
          type="number"
          value={contentId}
          onChange={(e) => setContentId(e.target.value)}
        />
        <Text
          name="log-search"
          label="Search prompt/response"
          width="14rem"
          value={search}
          onChange={(e) => setSearch(e.target.value)}
        />
        <Button
          variant={ButtonVariant.secondary}
          onClick={() => {
            setPage(1);
            setApplied((count) => count + 1);
          }}
        >
          Apply filters
        </Button>
        <Button variant={ButtonVariant.link} disabled={!data?.items.length} onClick={exportLog}>
          Export page
        </Button>
      </Row>
      <Show visible={loading}>
        <p className="v2-field-help">Loading…</p>
      </Show>
      <Show visible={!loading && (data?.items.length ?? 0) === 0}>
        <p className="modal-help-text">
          No log entries match. Logs are kept for the current date only.
        </p>
      </Show>
      <div className="v2-log-scroll">
        {(data?.items ?? []).map((entry) => (
          <LogEntry
            key={entry.id}
            entry={entry}
            expanded={expanded.has(entry.id)}
            explaining={explaining === entry.id}
            onToggle={() =>
              setExpanded((current) => {
                const next = new Set(current);
                if (next.has(entry.id)) next.delete(entry.id);
                else next.add(entry.id);
                return next;
              })
            }
            onToggleExplain={() =>
              setExplaining((current) => (current === entry.id ? null : entry.id))
            }
          >
            <Show visible={explaining === entry.id}>
              <ExplainPanel
                runId={runId}
                logId={entry.id}
                onExplain={onExplain}
                promptNames={promptNames}
                onApplyPrompt={onApplyPrompt}
              />
            </Show>
          </LogEntry>
        ))}
      </div>
      <Show visible={(data?.total ?? 0) > PAGE_SIZE}>
        <Row gap="0.5rem" alignItems="center">
          <button
            type="button"
            className="rule-icon-button"
            aria-label="Beginning of the log"
            title="Beginning of the log"
            disabled={page <= 1}
            onClick={() => setPage(1)}
          >
            <FaAnglesLeft />
          </button>
          <button
            type="button"
            className="rule-icon-button"
            aria-label="Previous page"
            title="Previous page"
            disabled={page <= 1}
            onClick={() => setPage((current) => current - 1)}
          >
            <FaAngleLeft />
          </button>
          <span>
            Page {page} of {totalPages} ({data?.total.toLocaleString()} entries)
          </span>
          <button
            type="button"
            className="rule-icon-button"
            aria-label="Next page"
            title="Next page"
            disabled={page >= totalPages}
            onClick={() => setPage((current) => current + 1)}
          >
            <FaAngleRight />
          </button>
          <button
            type="button"
            className="rule-icon-button"
            aria-label="End of the log"
            title="End of the log (newest entries)"
            disabled={page >= totalPages}
            onClick={() => setPage(totalPages)}
          >
            <FaAnglesRight />
          </button>
        </Row>
      </Show>
    </Col>
  );
};

interface ILogEntryProps {
  entry: IAutomationRunLogModel;
  expanded: boolean;
  explaining: boolean;
  onToggle: () => void;
  onToggleExplain: () => void;
  children?: React.ReactNode;
}

const LogEntry: React.FC<ILogEntryProps> = ({
  entry,
  expanded,
  explaining,
  onToggle,
  onToggleExplain,
  children,
}) => (
  <Col className={`v2-log-entry${entry.isLLM ? '' : ' v2-log-entry-decision'}`} gap="0.25rem">
    <Row gap="0.5rem" alignItems="center" nowrap className="v2-log-entry-header" onClick={onToggle}>
      <span className={v2OutcomeBadgeClass(entry.outcome)}>{entry.outcome}</span>
      <Show visible={!entry.isLLM}>
        <span className="v2-badge">engine</span>
      </Show>
      <Show visible={!!entry.variant}>
        <span className="v2-badge">variant {entry.variant}</span>
      </Show>
      <strong>{entry.stepName}</strong>
      <span>{entry.analysisName ?? entry.actionName ?? ''}</span>
      <Show visible={!!entry.contentId}>
        <span className="v2-field-help">content {entry.contentId}</span>
      </Show>
      <Show visible={entry.isLLM}>
        <span className="v2-field-help">
          {entry.promptTokens != null
            ? `${entry.promptTokens}+${entry.completionTokens ?? 0} tok`
            : ''}{' '}
          {entry.durationMs}ms{entry.attempt > 1 ? ` (attempt ${entry.attempt})` : ''}
        </span>
      </Show>
    </Row>
    <Show visible={expanded}>
      <Col gap="0.25rem" className="v2-log-entry-body">
        <Show visible={!!entry.prompt}>
          <details open={false}>
            <summary>Prompt</summary>
            <pre>{entry.prompt}</pre>
          </details>
        </Show>
        <Show visible={!!entry.response}>
          <label>{entry.isLLM ? 'Response' : 'Decision'}:</label>
          <pre>{entry.response}</pre>
        </Show>
        <Show visible={!!entry.detail}>
          <label>Detail:</label>
          <pre>{entry.detail}</pre>
        </Show>
        <Row>
          <Button variant={ButtonVariant.link} onClick={onToggleExplain}>
            {explaining ? 'Close explain' : 'Ask why / improve…'}
          </Button>
        </Row>
        {children}
      </Col>
    </Show>
  </Col>
);
