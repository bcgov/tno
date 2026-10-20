import React from 'react';
import { FaPaperPlane, FaWandMagicSparkles } from 'react-icons/fa6';

import {
  IFilterAIChatRequestModel,
  IFilterAIChatResultModel,
  IFilterAIGenerateRequestModel,
  IFilterAIGenerateResultModel,
  IFilterAIMessageModel,
  IFilterSettingsModel,
} from '../../hooks';
import { Modal } from '../modal';
import { Spinner } from '../spinners';
import { FilterAIButton, FilterAIButtonComponent } from './FilterAIButton';
import { FilterAIPromptEditor, FilterAIPromptEditorType } from './FilterAIPromptEditor';
import * as styled from './styled';
import { getFilterAIErrorMessage, getFilterSettingsSummary, htmlToPromptText } from './utils';

interface IThreadTurn {
  role: 'user' | 'assistant';
  text: string;
  suggestedPrompt?: string;
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
  /** Apply the generated filter and the prompt it came from. */
  onApply: (result: IFilterAIGenerateResultModel, prompt: string) => void;
  onClose: () => void;
  /** The button to render, so the app's own button styling is used (default: tno-core Button). */
  buttonComponent?: FilterAIButtonComponent;
}

/**
 * A workbench for a filter prompt: edit a draft, generate the filter it describes, read how the
 * AI built it, and ask the AI how to write the prompt. Nothing changes the filter until "Apply".
 */
export const FilterAIWorkbench: React.FC<IFilterAIWorkbenchProps> = ({
  open,
  prompt,
  editor = 'wysiwyg',
  settings,
  explanation,
  onGenerate,
  onChat,
  onApply,
  onClose,
  buttonComponent: ButtonComponent = FilterAIButton,
}) => {
  const [draftPrompt, setDraftPrompt] = React.useState('');
  const [tab, setTab] = React.useState<'result' | 'chat'>('result');
  const [result, setResult] = React.useState<IFilterAIGenerateResultModel>();
  const [resultPrompt, setResultPrompt] = React.useState<string>();
  const [isGenerating, setIsGenerating] = React.useState(false);
  const [error, setError] = React.useState<string>();
  const [messages, setMessages] = React.useState<IFilterAIMessageModel[]>([]);
  const [nonce, setNonce] = React.useState<string>();
  const [thread, setThread] = React.useState<IThreadTurn[]>([]);
  const [question, setQuestion] = React.useState('');
  const [isAsking, setIsAsking] = React.useState(false);
  const [chatError, setChatError] = React.useState<string>();

  // The draft starts from the filter's prompt each time the workbench opens, not on every render.
  const wasOpen = React.useRef(false);
  React.useEffect(() => {
    if (open && !wasOpen.current)
      setDraftPrompt(editor === 'textarea' ? htmlToPromptText(prompt) : prompt ?? '');
    wasOpen.current = open;
  }, [open, prompt, editor]);

  const generate = async () => {
    if (isGenerating) return;
    setIsGenerating(true);
    setError(undefined);
    try {
      const generated = await onGenerate({ prompt: draftPrompt, settings });
      setResult(generated);
      setResultPrompt(draftPrompt);
      setTab('result');
    } catch (ex) {
      setError(getFilterAIErrorMessage(ex));
    } finally {
      setIsGenerating(false);
    }
  };

  const ask = async () => {
    const text = question.trim();
    if (!text || isAsking) return;
    setThread((t) => [...t, { role: 'user', text }]);
    setQuestion('');
    setIsAsking(true);
    setChatError(undefined);
    try {
      const answer = await onChat({
        question: text,
        prompt: draftPrompt,
        settings: result?.settings ?? settings,
        explanation: result?.explanation ?? explanation,
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

  const isStale = !!result && resultPrompt !== draftPrompt;
  const summary = getFilterSettingsSummary(result?.settings);

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
          </div>
          <div className="results">
            <div className="tabs" role="tablist">
              <button
                type="button"
                role="tab"
                aria-selected={tab === 'result'}
                onClick={() => setTab('result')}
              >
                Result
              </button>
              <button
                type="button"
                role="tab"
                aria-selected={tab === 'chat'}
                onClick={() => setTab('chat')}
              >
                Chat
              </button>
            </div>
            <div className="panel">
              {tab === 'result' &&
                (result ? (
                  <>
                    {isStale && (
                      <p className="hint">
                        The draft has changed since this result was generated. Generate again to see
                        the filter for the current draft.
                      </p>
                    )}
                    {result.explanation && <p className="explanation">{result.explanation}</p>}
                    <h3>Settings</h3>
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
                      <p className="hint">No settings; every story matches.</p>
                    )}
                    {result.settings.extendedQuery && (
                      <>
                        <h3>Additional AI conditions</h3>
                        <p className="hint">
                          Raw Elasticsearch clauses for what the settings cannot express. They are
                          kept with the filter and added to its query.
                        </p>
                        <pre>{JSON.stringify(result.settings.extendedQuery, null, 2)}</pre>
                      </>
                    )}
                    {result.steps.length > 0 && (
                      <>
                        <h3>How the AI built it</h3>
                        {result.steps.map((step, i) => (
                          <details key={i}>
                            <summary>
                              {step.tool} {step.arguments}
                            </summary>
                            <pre>{step.result}</pre>
                          </details>
                        ))}
                      </>
                    )}
                  </>
                ) : (
                  <p>Generate the draft to see the filter it describes here.</p>
                ))}
              {tab === 'chat' && (
                <>
                  <p className="hint">
                    Ask how to describe the filter you want. The AI sees the draft prompt and the
                    {result ? ' last generated' : ' current'} filter, and can look up sources, tags
                    and other values and count matching stories.
                  </p>
                  <div className="thread">
                    {thread.map((turn, i) => (
                      <div key={i} className={`turn ${turn.role}`}>
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
                      onClick={ask}
                      disabled={isAsking || !question.trim()}
                      title="Send (Ctrl+Enter)"
                    >
                      <FaPaperPlane /> Send
                    </ButtonComponent>
                  </div>
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
            disabled={!result || isStale}
            title={
              !result
                ? 'Generate the filter first'
                : isStale
                ? 'Generate the filter for the current draft first'
                : 'Use this filter and prompt'
            }
            onClick={() => {
              if (!result || resultPrompt === undefined) return;
              onApply(result, resultPrompt);
              onClose();
            }}
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
