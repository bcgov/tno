import moment from 'moment';
import React from 'react';
import { FaPaperPlane, FaToggleOff, FaToggleOn, FaWandMagicSparkles } from 'react-icons/fa6';

import {
  IContentModel,
  IFilterAIChatRequestModel,
  IFilterAIChatResultModel,
  IFilterAIGenerateRequestModel,
  IFilterAIGenerateResultModel,
  IFilterAIMessageModel,
  IFilterAISearchResultModel,
  IFilterSettingsModel,
} from '../../hooks';
import { elasticsearchQuerySchema } from '../../utils';
import { ToggleButton } from '../button';
import { JsonEditor } from '../code-editor';
import { Modal } from '../modal';
import { Sentiment } from '../sentiment';
import { Spinner } from '../spinners';
import { FilterAIButton, FilterAIButtonComponent } from './FilterAIButton';
import { FilterAIPromptEditor, FilterAIPromptEditorType } from './FilterAIPromptEditor';
import * as styled from './styled';
import {
  getFilterAIErrorMessage,
  getFilterAIStoryStats,
  getFilterSettingsSummary,
  htmlToPromptText,
} from './utils';

interface IThreadTurn {
  role: 'user' | 'assistant';
  text: string;
  suggestedPrompt?: string;
  /** The headline of the listed story the question was about. */
  about?: string;
}

/** The questions offered about a selected story. */
const WHY_INCLUDED = 'Why is this story included in the results?';
const HOW_TO_EXCLUDE =
  'How can I exclude this story without losing the other stories the filter finds?';

type WorkbenchTab = 'chat' | 'result' | 'manual' | 'query';

/** The most stories the workbench lists for a query. */
const PREVIEW_SIZE = 500;
/** How long after the last edit the stories are refreshed. */
const PREVIEW_DELAY = 600;
/** The fields the listed stories need. */
const PREVIEW_FIELDS = [
  'id',
  'headline',
  'publishedOn',
  'source',
  'otherSource',
  'mediaType',
  'tonePools',
  'contentType',
  'section',
  'page',
];

export interface IFilterAIManualFilterProps {
  /** The settings the form shows. */
  settings: IFilterSettingsModel;
  /** The settings were edited. */
  onChange: (settings: IFilterSettingsModel) => void;
}

export interface IFilterAIWorkbenchProps {
  /** Whether the workbench is open. */
  open: boolean;
  /** The prompt the draft starts from each time the workbench opens. */
  prompt: string;
  /** The editor the draft is written in (default: wysiwyg). */
  editor?: FilterAIPromptEditorType;
  /** The filter's current settings. */
  settings: IFilterSettingsModel;
  /** The explanation from the last generation outside the workbench. */
  explanation?: string;
  /** Generate filter settings from a prompt. */
  onGenerate: (request: IFilterAIGenerateRequestModel) => Promise<IFilterAIGenerateResultModel>;
  /** Ask how to write the prompt. */
  onChat: (request: IFilterAIChatRequestModel) => Promise<IFilterAIChatResultModel>;
  /** Build the Elasticsearch query the app runs for the settings; undefined while the app is not ready. */
  onBuildQuery: (settings: IFilterSettingsModel) => any;
  /** Run a query and return the matching stories. */
  onSearch: (query: any, searchUnpublished: boolean) => Promise<IFilterAISearchResultModel>;
  /** Render the app's own filter form, bound to the settings being worked on. */
  renderManualFilter?: (props: IFilterAIManualFilterProps) => React.ReactNode;
  /** What becomes of an edited query when the filter is applied. */
  queryHint?: string;
  /** Apply the settings, the prompt they came from, and the query if it was edited by hand. */
  onApply: (result: IFilterAIGenerateResultModel, prompt: string, query?: any) => void;
  onClose: () => void;
  /** The button to render, so the app's own button styling is used (default: tno-core Button). */
  buttonComponent?: FilterAIButtonComponent;
}

const parseJson = (text: string): { value?: any; error?: string } => {
  try {
    return { value: JSON.parse(text) };
  } catch (ex) {
    return { error: ex instanceof Error ? ex.message : 'Invalid JSON' };
  }
};

/**
 * A workbench for a filter prompt: edit a draft, generate the filter it describes, adjust the
 * filter by hand or its query, see the stories it finds, and ask the AI how to write the prompt.
 * Nothing changes the filter until "Apply".
 */
export const FilterAIWorkbench: React.FC<IFilterAIWorkbenchProps> = ({
  open,
  prompt,
  editor = 'wysiwyg',
  settings,
  explanation,
  onGenerate,
  onChat,
  onBuildQuery,
  onSearch,
  renderManualFilter,
  queryHint = 'Edits here change the stories listed on the left.',
  onApply,
  onClose,
  buttonComponent: ButtonComponent = FilterAIButton,
}) => {
  const [draftPrompt, setDraftPrompt] = React.useState('');
  const [tab, setTab] = React.useState<WorkbenchTab>('chat');
  const [result, setResult] = React.useState<IFilterAIGenerateResultModel>();
  const [resultPrompt, setResultPrompt] = React.useState<string>();
  const [isGenerating, setIsGenerating] = React.useState(false);
  const [error, setError] = React.useState<string>();
  // The settings being worked on: the filter's, then the generated ones, then any manual edits.
  const [working, setWorking] = React.useState<IFilterSettingsModel>(settings);
  // Remounts the manual form when the settings are replaced, not when the form edits them.
  const [formKey, setFormKey] = React.useState(0);
  const [isEdited, setIsEdited] = React.useState(false);
  const [queryText, setQueryText] = React.useState('');
  const [isQueryEdited, setIsQueryEdited] = React.useState(false);
  const [stories, setStories] = React.useState<IFilterAISearchResultModel>();
  const [isSearching, setIsSearching] = React.useState(false);
  const [searchError, setSearchError] = React.useState<string>();
  const [messages, setMessages] = React.useState<IFilterAIMessageModel[]>([]);
  const [nonce, setNonce] = React.useState<string>();
  const [thread, setThread] = React.useState<IThreadTurn[]>([]);
  const [question, setQuestion] = React.useState('');
  const [isAsking, setIsAsking] = React.useState(false);
  const [chatError, setChatError] = React.useState<string>();
  // A listed story the user wants to ask the chat about.
  const [selectedStory, setSelectedStory] = React.useState<IContentModel>();

  // The app's callbacks are read through refs so that inline functions don't restart searches.
  const onBuildQueryRef = React.useRef(onBuildQuery);
  onBuildQueryRef.current = onBuildQuery;
  const onSearchRef = React.useRef(onSearch);
  onSearchRef.current = onSearch;

  const publishedOnly = !working.searchUnpublished;

  /** Replace the settings being worked on, i.e. with a generated result, and reset the form. */
  const replaceWorking = React.useCallback((next: IFilterSettingsModel) => {
    setWorking(next);
    setFormKey((k) => k + 1);
    setIsEdited(false);
  }, []);

  /** The form or the toggle changed the settings. */
  const updateWorking = React.useCallback((next: IFilterSettingsModel) => {
    setWorking((current) => {
      if (JSON.stringify(current) === JSON.stringify(next)) return current;
      setIsEdited(true);
      return next;
    });
  }, []);

  // The draft and the settings start from the filter each time the workbench opens, not on every render.
  const wasOpen = React.useRef(false);
  React.useEffect(() => {
    if (open && !wasOpen.current) {
      setDraftPrompt(editor === 'textarea' ? htmlToPromptText(prompt) : prompt ?? '');
      // The stories listed are published ones unless the toggle is switched.
      replaceWorking({ ...settings, searchUnpublished: false });
      setStories(undefined);
      setSelectedStory(undefined);
      setSearchError(undefined);
      setError(undefined);
    }
    wasOpen.current = open;
  }, [open, prompt, editor, settings, replaceWorking]);

  // The query follows the settings; an edited query is replaced when the settings change again.
  const builtQueryText = React.useMemo(() => {
    const query = onBuildQueryRef.current(working);
    return query ? JSON.stringify(query, null, 2) : '';
  }, [working]);
  React.useEffect(() => {
    setQueryText(builtQueryText);
    setIsQueryEdited(false);
  }, [builtQueryText]);

  const parsedQuery = React.useMemo(() => (queryText ? parseJson(queryText) : {}), [queryText]);

  // List the stories the query finds, a moment after the last change.
  const searchCount = React.useRef(0);
  React.useEffect(() => {
    if (!open) return;
    const query = parsedQuery.value;
    if (!query || typeof query !== 'object') return;
    const request = ++searchCount.current;
    const timer = setTimeout(async () => {
      setIsSearching(true);
      setSearchError(undefined);
      try {
        const response = await onSearchRef.current(
          { ...query, size: Math.min(query.size ?? 10, PREVIEW_SIZE), _source: PREVIEW_FIELDS },
          !publishedOnly,
        );
        if (request === searchCount.current) setStories(response);
      } catch (ex) {
        if (request === searchCount.current) {
          setStories(undefined);
          setSearchError(getFilterAIErrorMessage(ex));
        }
      } finally {
        if (request === searchCount.current) setIsSearching(false);
      }
    }, PREVIEW_DELAY);
    return () => clearTimeout(timer);
  }, [open, parsedQuery, publishedOnly]);

  const generate = async () => {
    if (isGenerating) return;
    setIsGenerating(true);
    setError(undefined);
    try {
      const generated = await onGenerate({ prompt: draftPrompt, settings: working });
      setResult(generated);
      setResultPrompt(draftPrompt);
      replaceWorking({ ...generated.settings, searchUnpublished: !publishedOnly });
      setTab('result');
    } catch (ex) {
      setError(getFilterAIErrorMessage(ex));
    } finally {
      setIsGenerating(false);
    }
  };

  /** Ask the typed question, or one of the questions offered about the selected story. */
  const ask = async (quickQuestion?: string) => {
    const text = (quickQuestion ?? question).trim();
    if (!text || isAsking) return;
    const about = selectedStory;
    setThread((t) => [...t, { role: 'user', text, about: about?.headline }]);
    if (!quickQuestion) setQuestion('');
    setIsAsking(true);
    setChatError(undefined);
    try {
      const answer = await onChat({
        question: text,
        prompt: draftPrompt,
        settings: working,
        explanation: result?.explanation ?? explanation,
        // The story the question is about, and the query that found it, so the answer can say why.
        contentId: about?.id,
        query: about && parsedQuery.value ? parsedQuery.value : undefined,
        nonce,
        messages,
      });
      setMessages(answer.messages);
      setNonce(answer.nonce);
      setThread((t) => [
        ...t,
        { role: 'assistant', text: answer.answer, suggestedPrompt: answer.suggestedPrompt },
      ]);
    } catch (ex) {
      setThread((t) => t.slice(0, -1));
      setQuestion(text);
      setChatError(getFilterAIErrorMessage(ex));
    } finally {
      setIsAsking(false);
    }
  };

  const apply = () => {
    onApply(
      { settings: working, explanation: result?.explanation ?? '', steps: result?.steps ?? [] },
      draftPrompt,
      isQueryEdited && parsedQuery.value ? parsedQuery.value : undefined,
    );
    onClose();
  };

  const isStale = !!result && resultPrompt !== draftPrompt;
  const canApply = !!result || isEdited;
  const summary = getFilterSettingsSummary(working);
  const hits = React.useMemo(
    () =>
      (stories?.hits.hits ?? [])
        .map((hit) => hit._source)
        .filter((content): content is IContentModel => !!content),
    [stories],
  );
  const stats = React.useMemo(() => getFilterAIStoryStats(stories), [stories]);

  const tabs: { id: WorkbenchTab; label: string; show?: boolean }[] = [
    { id: 'chat', label: 'Chat' },
    { id: 'result', label: 'Result' },
    { id: 'manual', label: 'Manual Filter', show: !!renderManualFilter },
    { id: 'query', label: 'Elasticsearch Query' },
  ];

  return (
    <Modal
      isShowing={open}
      hide={onClose}
      headerText="Filter prompt workbench"
      type="custom"
      component={
        <styled.FilterAIWorkbench className="filter-ai-workbench">
          <div className="draft">
            <FilterAIPromptEditor
              name="filterAIDraftPrompt"
              editor={editor}
              label="Draft prompt"
              value={draftPrompt}
              onChange={setDraftPrompt}
            />
            <div className="actions">
              <ButtonComponent
                variant="primary"
                onClick={generate}
                disabled={isGenerating}
                title="Generate the filter this draft describes"
              >
                <FaWandMagicSparkles /> Generate
              </ButtonComponent>
              <ToggleButton
                className="published-only"
                on={<FaToggleOn />}
                off={<FaToggleOff />}
                value={publishedOnly}
                label="Published content only"
                labelPosition="right"
                width="1.5rem"
                height="1.5rem"
                title={
                  publishedOnly
                    ? 'Searching stories published by MMI editors; switch off to search all content, including drafts'
                    : 'Searching all content, including drafts; switch on for published stories only'
                }
                onClick={() => updateWorking({ ...working, searchUnpublished: publishedOnly })}
              />
              {isGenerating && (
                <span className="status" role="status">
                  <Spinner size="1rem" /> Generating the filter…
                </span>
              )}
              {error && (
                <span className="status failed" role="alert">
                  {error}
                </span>
              )}
            </div>
            <p className="hint">
              Describe the stories the filter should find: topics and keywords, sources, media
              types, programs, dates, tags. The AI turns the prompt into filter settings, and adds
              raw Elasticsearch conditions only for what the settings cannot express. Nothing
              changes until you apply the result.
            </p>
            <section className="stories" aria-label="Stories the filter finds">
              <div className="stats">
                <span className="count">
                  {isSearching ? (
                    <>
                      <Spinner size="1rem" /> Searching…
                    </>
                  ) : stories ? (
                    <>
                      <strong>
                        {stats.total.toLocaleString()}
                        {stats.totalIsLowerBound ? '+' : ''}
                      </strong>{' '}
                      {stats.total === 1 ? 'story' : 'stories'}
                      {stats.total > hits.length && <> · showing first {hits.length}</>}
                    </>
                  ) : (
                    'No search yet'
                  )}
                </span>
                {stories && hits.length > 0 && (
                  <>
                    {stats.earliest && stats.latest && (
                      <span className="stat" title="Published between">
                        {moment(stats.earliest).format('YYYY-MM-DD')}
                        {stats.earliest !== stats.latest && (
                          <> – {moment(stats.latest).format('YYYY-MM-DD')}</>
                        )}
                      </span>
                    )}
                    <span className="stat" title="Distinct sources">
                      {stats.sources} {stats.sources === 1 ? 'source' : 'sources'}
                    </span>
                    {stats.mediaTypes.map(([mediaType, count]) => (
                      <span key={mediaType} className="stat" title="Media type">
                        {mediaType}: {count}
                      </span>
                    ))}
                    <span
                      className="stat sentiment-stat"
                      title="Sentiment: positive, neutral, negative"
                    >
                      <Sentiment value={1} /> {stats.sentiment.positive}
                      <Sentiment value={0} /> {stats.sentiment.neutral}
                      <Sentiment value={-1} /> {stats.sentiment.negative}
                    </span>
                  </>
                )}
              </div>
              {selectedStory && (
                <div className="selected-story">
                  <span className="label">Selected:</span>
                  <span className="headline" title={selectedStory.headline}>
                    {selectedStory.headline}
                  </span>
                  <ButtonComponent
                    variant="secondary"
                    disabled={isAsking}
                    title="Ask the chat why the filter finds this story"
                    onClick={() => {
                      setTab('chat');
                      ask(WHY_INCLUDED);
                    }}
                  >
                    Why is it included?
                  </ButtonComponent>
                  <button
                    type="button"
                    className="link"
                    onClick={() => setSelectedStory(undefined)}
                    title="Stop asking about this story"
                  >
                    Clear
                  </button>
                </div>
              )}
              {searchError && (
                <p className="status failed" role="alert">
                  {searchError}
                </p>
              )}
              {parsedQuery.error && (
                <p className="status failed" role="alert">
                  The query is not valid JSON: {parsedQuery.error}
                </p>
              )}
              <ol
                className="list"
                role="listbox"
                aria-label="Stories the filter finds; select one to ask the chat about it"
              >
                {hits.map((content) => (
                  <li
                    key={content.id}
                    role="option"
                    aria-selected={selectedStory?.id === content.id}
                    className={selectedStory?.id === content.id ? 'selected' : undefined}
                    tabIndex={0}
                    onClick={() =>
                      setSelectedStory((s) => (s?.id === content.id ? undefined : content))
                    }
                    onKeyDown={(e) => {
                      if (e.key === 'Enter' || e.key === ' ') {
                        e.preventDefault();
                        setSelectedStory((s) => (s?.id === content.id ? undefined : content));
                      }
                    }}
                  >
                    <span
                      className="published"
                      title={
                        content.publishedOn
                          ? `Published ${moment(content.publishedOn).format('YYYY-MM-DD HH:mm')}`
                          : 'Published on'
                      }
                    >
                      {content.publishedOn ? moment(content.publishedOn).format('YYYY-MM-DD') : ''}
                    </span>
                    <span className="source" title={content.source?.name ?? content.otherSource}>
                      {content.source?.code ?? content.otherSource}
                    </span>
                    <span className="headline" title={content.headline}>
                      {content.headline}
                    </span>
                    <Sentiment
                      value={content.tonePools?.[0]?.value}
                      title={
                        content.tonePools?.[0]
                          ? `Sentiment ${content.tonePools[0].value}`
                          : 'No sentiment'
                      }
                    />
                  </li>
                ))}
              </ol>
            </section>
          </div>
          <div className="results">
            <div className="tabs" role="tablist">
              {tabs
                .filter((t) => t.show !== false)
                .map((t) => (
                  <button
                    key={t.id}
                    type="button"
                    role="tab"
                    aria-selected={tab === t.id}
                    onClick={() => setTab(t.id)}
                  >
                    {t.label}
                  </button>
                ))}
            </div>
            <div className={`panel ${tab}`}>
              {tab === 'chat' && (
                <>
                  <p className="hint">
                    Ask how to describe the filter you want. The AI sees the draft prompt and the
                    {result ? ' last generated' : ' current'} filter, and can look up sources, tags
                    and other values and count matching stories. Select a story on the left to ask
                    why it is included, and how to exclude it.
                  </p>
                  <div className="thread">
                    {thread.map((turn, i) => (
                      <div key={i} className={`turn ${turn.role}`}>
                        {turn.about && <div className="about">About: {turn.about}</div>}
                        {turn.text}
                        {turn.suggestedPrompt && (
                          <div className="suggested">
                            <div dangerouslySetInnerHTML={{ __html: turn.suggestedPrompt }} />
                            <ButtonComponent
                              variant="secondary"
                              onClick={() =>
                                setDraftPrompt(
                                  editor === 'textarea'
                                    ? htmlToPromptText(turn.suggestedPrompt)
                                    : turn.suggestedPrompt ?? '',
                                )
                              }
                            >
                              Use this prompt
                            </ButtonComponent>
                          </div>
                        )}
                      </div>
                    ))}
                    {isAsking && (
                      <div className="turn assistant">
                        <Spinner size="1rem" />
                      </div>
                    )}
                  </div>
                  {chatError && (
                    <p className="status failed" role="alert">
                      {chatError}
                    </p>
                  )}
                  {selectedStory && (
                    <div className="about-story">
                      <span>
                        Asking about: <strong>{selectedStory.headline}</strong>
                      </span>
                      <div className="quick">
                        <ButtonComponent
                          variant="secondary"
                          disabled={isAsking}
                          onClick={() => ask(WHY_INCLUDED)}
                        >
                          Why is it included?
                        </ButtonComponent>
                        <ButtonComponent
                          variant="secondary"
                          disabled={isAsking}
                          onClick={() => ask(HOW_TO_EXCLUDE)}
                        >
                          How do I exclude it?
                        </ButtonComponent>
                        <button
                          type="button"
                          className="link"
                          onClick={() => setSelectedStory(undefined)}
                        >
                          Clear
                        </button>
                      </div>
                    </div>
                  )}
                  <div className="ask">
                    <textarea
                      aria-label="Question"
                      placeholder="How do I find stories that…"
                      value={question}
                      onChange={(e) => setQuestion(e.target.value)}
                      onKeyDown={(e) => {
                        if (e.key === 'Enter' && (e.ctrlKey || e.metaKey)) ask();
                      }}
                    />
                    <ButtonComponent
                      variant="primary"
                      onClick={() => ask()}
                      disabled={isAsking || !question.trim()}
                      title="Send (Ctrl+Enter)"
                    >
                      <FaPaperPlane /> Send
                    </ButtonComponent>
                  </div>
                </>
              )}
              {tab === 'result' && (
                <>
                  {result ? (
                    <>
                      {isStale && (
                        <p className="hint">
                          The draft has changed since this result was generated. Generate again to
                          see the filter for the current draft.
                        </p>
                      )}
                      {result.explanation && <p className="explanation">{result.explanation}</p>}
                    </>
                  ) : (
                    <p className="hint">
                      Generate the draft to see how the AI describes the filter here. The search
                      criteria below are the filter's current settings.
                    </p>
                  )}
                  {isEdited && (
                    <p className="hint">The search criteria include your manual changes.</p>
                  )}
                  <h3>Search Criteria</h3>
                  {summary.length ? (
                    <dl className="settings">
                      {summary.map(([key, value]) => (
                        <React.Fragment key={key}>
                          <dt>{key}</dt>
                          <dd>{value}</dd>
                        </React.Fragment>
                      ))}
                    </dl>
                  ) : (
                    <p className="hint">No criteria; every story matches.</p>
                  )}
                  {working.extendedQuery && (
                    <>
                      <h3>Additional AI conditions</h3>
                      <p className="hint">
                        Raw Elasticsearch clauses for what the settings cannot express. They are
                        kept with the filter and added to its query.
                      </p>
                      <pre>{JSON.stringify(working.extendedQuery, null, 2)}</pre>
                    </>
                  )}
                </>
              )}
              {tab === 'manual' && renderManualFilter && (
                <>
                  <p className="hint">
                    The filter as the AI built it. Change anything to see the effect on the stories
                    listed on the left; generating again replaces these settings.
                  </p>
                  <div className="manual-filter" key={formKey}>
                    {renderManualFilter({ settings: working, onChange: updateWorking })}
                  </div>
                </>
              )}
              {tab === 'query' && (
                <>
                  <p className="hint">
                    The query the filter sends to Elasticsearch. {queryHint} Changing the filter
                    rebuilds it.
                  </p>
                  {builtQueryText || queryText ? (
                    <JsonEditor
                      name="filter-ai-query"
                      value={queryText}
                      schema={elasticsearchQuerySchema}
                      onChange={(text) => {
                        setQueryText(text);
                        setIsQueryEdited(true);
                        setIsEdited(true);
                      }}
                    />
                  ) : (
                    <p className="hint">The query cannot be built until the app has loaded.</p>
                  )}
                </>
              )}
            </div>
          </div>
        </styled.FilterAIWorkbench>
      }
      customButtons={
        <>
          <ButtonComponent
            variant="primary"
            disabled={!canApply}
            title={
              !canApply
                ? 'Generate the filter or change it first'
                : isStale
                ? 'Use this filter with the current draft prompt'
                : 'Use this filter and prompt'
            }
            onClick={apply}
          >
            Apply filter
          </ButtonComponent>
          <ButtonComponent variant="secondary" onClick={onClose}>
            Close
          </ButtonComponent>
        </>
      }
    />
  );
};
