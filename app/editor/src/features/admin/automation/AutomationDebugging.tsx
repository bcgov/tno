import { AdvancedSearchKeys } from 'features/content/constants';
import { useElasticsearch } from 'features/content/hooks';
import React from 'react';
import { toast } from 'react-toastify';
import { useContent } from 'store/hooks';
import { useAutomationProfiles } from 'store/hooks/admin';
import { Button, ButtonVariant, Col, Row, Show, Text, Wysiwyg } from 'tno-core';

import { type IAutomationDebugMessageModel } from './interfaces';

interface IContentHit {
  id: number;
  headline: string;
  source: string;
}

interface IAutomationDebuggingProps {
  /** The profile whose LLM and last run are used to answer the question. */
  profileId: number;
}

const DEFAULT_QUESTION = 'Why was the following content item not published?';

/** Collapse messages taller than this many lines by default. */
const COLLAPSE_LINES = 8;

/** Strip HTML tags for readable display of a WYSIWYG value. */
const stripHtml = (html: string) =>
  html
    .replace(/<[^>]+>/g, ' ')
    .replace(/\s+/g, ' ')
    .trim();

/** Whether a message is long enough to warrant a collapse/expand toggle. */
const isCollapsible = (text: string) =>
  text.split('\n').length > COLLAPSE_LINES || text.length > 400;

/**
 * Debugging section for the automation profile form. Lets the user pick a content item and ask the
 * profile's LLM why it was (or was not) acted upon, then continue the conversation with follow-up
 * questions. The full generated first prompt appears as the first message in the history. Each
 * message can be expanded/collapsed; long messages are collapsed by default. "Start new chat" resets
 * the conversation to pick a different content item.
 */
export const AutomationDebugging: React.FC<IAutomationDebuggingProps> = ({ profileId }) => {
  const [, api] = useAutomationProfiles();
  const [, { findContentWithElasticsearch }] = useContent();
  const toFilter = useElasticsearch();

  // Content search (first turn only).
  const [idTerm, setIdTerm] = React.useState('');
  const [term, setTerm] = React.useState('');
  const [results, setResults] = React.useState<IContentHit[]>([]);
  const [selected, setSelected] = React.useState<IContentHit | null>(null);
  const [isSearching, setIsSearching] = React.useState(false);

  // Conversation.
  const [input, setInput] = React.useState(DEFAULT_QUESTION);
  const [messages, setMessages] = React.useState<IAutomationDebugMessageModel[]>([]);
  const [pending, setPending] = React.useState<string | null>(null);
  const [expanded, setExpanded] = React.useState<Set<number>>(new Set());
  const [isAsking, setIsAsking] = React.useState(false);

  // Displayable history (the system message is context only and is not shown).
  const history = messages.filter((m) => m.role !== 'system');
  const hasConversation = history.length > 0;

  const toggleExpanded = (index: number) => {
    setExpanded((prev) => {
      const next = new Set(prev);
      if (next.has(index)) next.delete(index);
      else next.add(index);
      return next;
    });
  };

  const handleSearch = async () => {
    // An id finds the one item directly; otherwise the headline term searches.
    const byId = idTerm.trim();
    if (!byId && !term.trim()) return;
    setIsSearching(true);
    try {
      const filter = {
        pageIndex: 0,
        pageSize: 10,
        onlyPublished: false,
        hasTopic: false,
        isHidden: '',
        ownerId: '',
        userId: '',
        timeFrame: '',
        contentTypes: [],
        mediaTypeIds: [],
        sourceIds: [],
        excludeSourceIds: [],
        sort: [],
        fieldType: byId ? AdvancedSearchKeys.Id : AdvancedSearchKeys.Headline,
        searchTerm: byId || term,
      };
      const response = await findContentWithElasticsearch(toFilter(filter as any), true);
      const hits = (response.hits?.hits ?? [])
        .filter((h) => !!h._source)
        .map((h) => {
          const s = h._source!;
          return {
            id: s.id,
            headline: s.headline ?? '(no headline)',
            source: s.source?.name ?? s.otherSource ?? '',
          };
        });
      setResults(hits);
      if (!hits.length) toast.info('No content items matched that search.');
    } catch {
      toast.error('Content search failed.');
    } finally {
      setIsSearching(false);
    }
  };

  const handleSend = async () => {
    const text = stripHtml(input);
    if (!text) {
      toast.warning('Enter a question.');
      return;
    }
    setIsAsking(true);
    setPending(text); // optimistic echo while awaiting the response
    try {
      const result = await api.debugContent(profileId, {
        contentId: selected?.id ?? 0,
        question: input,
        messages,
      });
      setMessages(result.messages ?? []);
      setInput('');
    } catch {
      toast.error('The debugging request failed.');
    } finally {
      setPending(null);
      setIsAsking(false);
    }
  };

  const handleNewChat = () => {
    setMessages([]);
    setPending(null);
    setExpanded(new Set());
    setInput(DEFAULT_QUESTION);
    setSelected(null);
    setResults([]);
    setTerm('');
    setIdTerm('');
  };

  const renderMessage = (role: string, text: string, index: number, collapsible: boolean) => {
    const isExpanded = expanded.has(index);
    return (
      <div key={index} className={`debug-message ${role === 'user' ? 'user' : 'assistant'}`}>
        <div className="role">{role === 'user' ? 'Prompt' : 'Response'}</div>
        <pre
          className={`text${collapsible && !isExpanded ? ' collapsed' : ''}`}
          style={{ whiteSpace: 'pre-wrap', wordBreak: 'break-word', margin: 0 }}
        >
          {text}
        </pre>
        <Show visible={collapsible}>
          <button type="button" className="debug-toggle" onClick={() => toggleExpanded(index)}>
            {isExpanded ? 'Show less' : 'Show more'}
          </button>
        </Show>
      </div>
    );
  };

  return (
    <Col className="debugging-section" gap="1rem">
      <p>
        Ask the profile&apos;s LLM about the most recent run. The first message sent (shown in the
        history) combines your question with how the profile works, its configuration, and the
        run&apos;s outcome — plus a specific content item when you pick one below; follow-up
        questions continue the same conversation.
      </p>

      {/* 1. Find the content item (first turn only). */}
      <Show visible={!hasConversation && !isAsking}>
        <Col gap="0.5rem">
          <label>
            Find content by id or headline (optional — focuses the question on one item)
          </label>
          <Row gap="0.5rem" alignItems="center">
            <Text
              name="debug-search-id"
              value={idTerm}
              width="8rem"
              placeholder="Id"
              onChange={(e) => setIdTerm(e.target.value.replace(/[^0-9]/g, ''))}
              onKeyDown={(e) => {
                if (e.key === 'Enter') {
                  e.preventDefault();
                  handleSearch();
                }
              }}
            />
            <Text
              name="debug-search"
              value={term}
              placeholder="Search headlines…"
              onChange={(e) => setTerm(e.target.value)}
              onKeyDown={(e) => {
                if (e.key === 'Enter') {
                  e.preventDefault();
                  handleSearch();
                }
              }}
            />
            <Row className="automation-log-filter-actions">
              <Button
                type="button"
                variant={ButtonVariant.secondary}
                disabled={isSearching || (!idTerm.trim() && !term.trim())}
                onClick={handleSearch}
              >
                Search
              </Button>
            </Row>
          </Row>
          <Show visible={results.length > 0}>
            <ul className="debug-results">
              {results.map((r) => (
                <li key={r.id}>
                  <button
                    type="button"
                    className={`debug-result${selected?.id === r.id ? ' selected' : ''}`}
                    onClick={() => setSelected(r)}
                  >
                    <span className="id">#{r.id}</span> {r.headline}
                    {r.source ? <span className="source"> — {r.source}</span> : null}
                  </button>
                </li>
              ))}
            </ul>
          </Show>
        </Col>
      </Show>

      <Show visible={!!selected}>
        <div className="debug-selected">
          Content: <strong>#{selected?.id}</strong> {selected?.headline}
        </div>
      </Show>

      {/* 2. Conversation transcript (generated prompt is the first message). */}
      <Show visible={hasConversation || isAsking}>
        <Col className="debug-transcript" gap="0.75rem">
          {history.map((m, index) =>
            renderMessage(m.role, m.content, index, isCollapsible(m.content)),
          )}
          <Show visible={isAsking}>
            {pending ? renderMessage('user', pending, -1, false) : null}
            <div className="debug-message assistant">
              <div className="role">Response</div>
              <div className="text">Thinking…</div>
            </div>
          </Show>
        </Col>
      </Show>

      {/* 3. Prompt input (used for the first question and every follow-up). */}
      <Wysiwyg
        name="debug-question"
        label={hasConversation ? 'Follow-up' : 'Question'}
        value={input}
        onChange={(value) => setInput(value ?? '')}
      />

      {/* 4. Actions. */}
      <Row justifyContent="space-between" alignItems="center">
        <Show visible={hasConversation}>
          <Button type="button" variant={ButtonVariant.secondary} onClick={handleNewChat}>
            Start new chat
          </Button>
        </Show>
        <Show visible={!hasConversation}>
          <span />
        </Show>
        <Button type="button" disabled={isAsking} onClick={handleSend}>
          {isAsking ? 'Sending…' : hasConversation ? 'Send' : 'Ask the LLM'}
        </Button>
      </Row>
    </Col>
  );
};
