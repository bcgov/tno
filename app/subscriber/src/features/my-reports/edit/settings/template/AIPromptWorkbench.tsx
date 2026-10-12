import { Button } from 'components/button';
import React from 'react';
import { FaCode, FaPaperPlane, FaPlay } from 'react-icons/fa6';
import { useApiHub, useReportAI } from 'store/hooks';
import {
  IReportAIPromptChatMessageModel,
  IReportAISectionPromptModel,
  IReportAISectionTestResultModel,
} from 'store/hooks/subscriber/interfaces';
import styled from 'styled-components';
import {
  IReportMessageModel,
  IReportSectionSettingsModel,
  MessageTargetKey,
  Spinner,
  Wysiwyg,
} from 'tno-core';

import { AIPromptDialog, AIPromptTranscript } from '../../../components';
import { ReportBody } from '../../../ReportBody';
import { getAISectionTestId } from '../../../utils';
import { useReportEditContext } from '../../ReportEditContext';

/** How often a pending run is checked when no hub notice arrives. */
export const AI_TEST_POLL_MS = 15000;
/** How long the API keeps a run; after that the workbench stops asking. */
export const AI_TEST_LIFETIME_MS = 30 * 60 * 1000;

/**
 * The dialog body is a column so the workbench can take the remaining height and keep its
 * columns inside it; the body then only scrolls on narrow screens, where the columns stack.
 */
const WorkbenchDialog = styled(AIPromptDialog)`
  .ai-dialog-body {
    display: flex;
    flex-direction: column;
  }
`;

const Workbench = styled.div`
  display: grid;
  grid-template-columns: minmax(20rem, 1fr) minmax(24rem, 1.3fr);
  grid-template-rows: minmax(0, 1fr);
  gap: 1.25rem;
  flex: 1 1 auto;
  min-height: 60vh;

  @media (max-width: 70rem) {
    grid-template-columns: 1fr;
    grid-template-rows: auto;
  }

  .draft {
    display: flex;
    flex-direction: column;
    gap: 0.5rem;
    min-width: 0;
    min-height: 0;

    /* The editor fills the column and scrolls its own text, instead of growing the dialog. */
    > div:has(.ql-container) {
      display: flex;
      flex-direction: column;
      flex: 1 1 auto;
      min-height: 0;
    }

    .editor {
      display: flex;
      flex-direction: column;
      flex: 1 1 auto;
      min-height: 0;
    }

    .ql-container {
      flex: 1 1 auto;
      height: auto;
      min-height: 18rem;
      overflow: hidden;
    }

    .ql-editor {
      overflow-y: auto;
    }
  }

  .actions {
    display: flex;
    flex-wrap: wrap;
    align-items: center;
    gap: 0.5rem;
  }

  .status {
    display: flex;
    align-items: center;
    gap: 0.5rem;
    font-size: 0.9rem;

    &.failed {
      color: #b00020;
    }
  }

  .usage {
    font-size: 0.85rem;
    color: #555;
  }

  .results {
    display: flex;
    flex-direction: column;
    gap: 0.5rem;
    min-width: 0;
  }

  .tabs {
    display: flex;
    gap: 0.25rem;
    border-bottom: 1px solid #ddd;

    button {
      padding: 0.4rem 0.9rem;
      background: transparent;
      border: 1px solid transparent;
      border-bottom: none;
      border-radius: 0.35rem 0.35rem 0 0;
      cursor: pointer;

      &[aria-selected='true'] {
        border-color: #ddd;
        background: #fff;
        font-weight: 600;
        margin-bottom: -1px;
      }
    }
  }

  .panel {
    flex: 1 1 auto;
    min-height: 0;
    overflow: auto;
    padding: 0.5rem 0.25rem;
  }

  .output {
    border: 1px solid #eee;
    padding: 0.75rem;
    background: #fff;
  }

  .thread {
    display: flex;
    flex-direction: column;
    gap: 0.75rem;

    .turn {
      padding: 0.6rem 0.8rem;
      border-radius: 0.5rem;
      white-space: pre-wrap;
      overflow-wrap: anywhere;

      &.user {
        align-self: flex-end;
        background: #e8eef9;
        max-width: 85%;
      }

      &.assistant {
        align-self: flex-start;
        background: #f3f3f3;
        max-width: 95%;
      }

      .suggested {
        margin-top: 0.5rem;
        padding: 0.5rem;
        border: 1px dashed #999;
        background: #fff;
        white-space: normal;
      }
    }
  }

  .ask {
    display: flex;
    gap: 0.5rem;
    align-items: flex-end;

    textarea {
      flex: 1;
      min-height: 4rem;
      padding: 0.5rem;
      border: 1px solid #bbb;
      border-radius: 0.25rem;
      font: inherit;
    }
  }

  .hint {
    margin: 0;
    font-size: 0.85rem;
    color: #555;
  }
`;

interface IThreadTurn {
  role: 'user' | 'assistant';
  text: string;
  suggestedPrompt?: string;
}

export interface IAIPromptWorkbenchProps {
  /** The AI section's index in the report form. */
  index: number;
  open: boolean;
  onClose: () => void;
}

/**
 * A workbench for one AI section's prompt: edit a draft, run it against the report's current
 * content, read the output and the full transcript, and ask the model how to change the prompt.
 * The draft is written to the section only with "Apply to section".
 */
export const AIPromptWorkbench: React.FC<IAIPromptWorkbenchProps> = ({ index, open, onClose }) => {
  const { values, setFieldValue, isSubmitting } = useReportEditContext();
  const { testSection, getTest, deleteTest, describeSection, chat } = useReportAI();
  const hub = useApiHub();

  const section = values.sections[index];
  const sectionId = section?.id ?? 0;
  const reportId = values.id;
  const hasInstance = (values.instances?.length ?? 0) > 0;

  const [draftPrompt, setDraftPrompt] = React.useState('');
  const [test, setTest] = React.useState<IReportAISectionTestResultModel>();
  // The run is kept for this workbench only: drop it on the way out of the editor page.
  const latest = React.useRef({ reportId, sectionId, testId: test?.testId, deleteTest });
  latest.current = { reportId, sectionId, testId: test?.testId, deleteTest };
  React.useEffect(
    () => () => {
      const { reportId, sectionId, testId, deleteTest } = latest.current;
      if (testId && reportId && sectionId)
        Promise.resolve(deleteTest(reportId, sectionId, testId)).catch(() => {});
    },
    [],
  );
  const [tab, setTab] = React.useState<'output' | 'transcript' | 'chat'>('output');
  const [plan, setPlan] = React.useState<IReportAISectionPromptModel>();
  const [planFor, setPlanFor] = React.useState<string>();
  const [isPlanning, setIsPlanning] = React.useState(false);
  const [showPlanned, setShowPlanned] = React.useState(false);
  const [messages, setMessages] = React.useState<IReportAIPromptChatMessageModel[]>([]);
  const [thread, setThread] = React.useState<IThreadTurn[]>([]);
  const [question, setQuestion] = React.useState('');
  const [isAsking, setIsAsking] = React.useState(false);

  // The draft starts from the section each time the workbench opens, not on every render.
  const wasOpen = React.useRef(false);
  React.useEffect(() => {
    if (open && !wasOpen.current) setDraftPrompt(section?.settings.userPrompt ?? '');
    wasOpen.current = open;
  }, [open, section?.settings.userPrompt]);

  const draftSettings = React.useMemo<IReportSectionSettingsModel>(
    () => ({ ...section?.settings, userPrompt: draftPrompt }),
    [section?.settings, draftPrompt],
  );

  const isPending = test?.status === 'Pending';
  const disabledReason =
    !reportId || !sectionId
      ? 'Save the report to test this section'
      : !hasInstance
      ? 'Generate the report to test this section'
      : isSubmitting
      ? 'The report is being saved'
      : isPending
      ? 'A test is already running'
      : undefined;

  const refresh = React.useCallback(async () => {
    if (!test || !reportId || !sectionId) return;
    try {
      const result = await getTest(reportId, sectionId, test.testId);
      if (result) setTest(result);
      else
        setTest((current) =>
          current && current.testId === test.testId
            ? {
                ...current,
                status: 'Failed',
                error: 'The test run is no longer available. Generate it again.',
              }
            : current,
        );
    } catch {}
  }, [getTest, reportId, sectionId, test]);

  // The reporting service announces the run on the report status channel.
  const onHubMessage = React.useCallback(
    async (message: IReportMessageModel) => {
      if (isPending && getAISectionTestId(message) === test?.testId) await refresh();
    },
    [isPending, refresh, test?.testId],
  );
  hub.useHubEffect(MessageTargetKey.ReportStatus, onHubMessage);

  // Notices can be missed; check a pending run now and then, and give up when the API would have forgotten it.
  React.useEffect(() => {
    if (!open || !isPending || !test) return;
    const timer = setInterval(() => {
      if (Date.now() - new Date(test.createdOn).getTime() > AI_TEST_LIFETIME_MS)
        setTest((current) =>
          current && current.testId === test.testId
            ? { ...current, status: 'Failed', error: 'The test run timed out. Generate it again.' }
            : current,
        );
      else refresh();
    }, AI_TEST_POLL_MS);
    return () => clearInterval(timer);
  }, [open, isPending, test, refresh]);

  const generate = async () => {
    if (disabledReason) return;
    try {
      const result = await testSection(reportId, sectionId, draftSettings);
      setTest(result);
      setShowPlanned(false);
      setTab('output');
    } catch {}
  };

  // The planned prompt for the current draft, fetched when the transcript tab needs it.
  const draftKey = JSON.stringify(draftSettings);
  const hasActual = !!test?.transcript?.length && test.status !== 'Pending';
  const wantsPlan = open && tab === 'transcript' && (!hasActual || showPlanned);
  // Only the newest request may settle the state: an older one, superseded while the draft
  // changed, is ignored rather than left to clear the loading flag out of turn.
  const planRequest = React.useRef(0);
  React.useEffect(() => {
    if (!wantsPlan || !reportId || !sectionId || planFor === draftKey) return;
    const request = ++planRequest.current;
    setIsPlanning(true);
    describeSection(reportId, sectionId, JSON.parse(draftKey))
      .then((result) => {
        if (request === planRequest.current) setPlan(result);
      })
      .catch(() => {})
      .finally(() => {
        if (request !== planRequest.current) return;
        setPlanFor(draftKey);
        setIsPlanning(false);
      });
  }, [wantsPlan, draftKey, planFor, reportId, sectionId, describeSection]);

  const ask = async () => {
    const text = question.trim();
    if (!text || isAsking || !reportId || !sectionId) return;
    setThread((t) => [...t, { role: 'user', text }]);
    setQuestion('');
    setIsAsking(true);
    try {
      const result = await chat(reportId, sectionId, {
        settings: draftSettings,
        question: text,
        messages,
        testId: test?.testId,
      });
      setMessages(result.messages);
      setThread((t) => [
        ...t,
        { role: 'assistant', text: result.answer, suggestedPrompt: result.suggestedPrompt },
      ]);
    } catch {
      setThread((t) => t.slice(0, -1));
      setQuestion(text);
    } finally {
      setIsAsking(false);
    }
  };

  const apply = () => {
    setFieldValue(`sections.${index}.settings.userPrompt`, draftPrompt);
    onClose();
  };

  if (!section) return null;

  return (
    <WorkbenchDialog
      open={open}
      title={`Prompt workbench: ${section.settings.label || section.name}`}
      onClose={onClose}
      width="min(110rem, 96vw)"
      footer={
        <>
          <Button variant="secondary" onClick={onClose}>
            Close
          </Button>
          <Button
            variant="primary"
            onClick={apply}
            title="Write the draft into the section's prompt"
          >
            Apply to section
          </Button>
        </>
      }
    >
      <Workbench className="ai-prompt-workbench">
        <div className="draft">
          <Wysiwyg
            name={`draftPrompt-${index}`}
            label="Draft prompt"
            value={draftPrompt}
            onChange={(html: string) => setDraftPrompt(html)}
          />
          <div className="actions">
            <Button
              variant="primary"
              onClick={generate}
              disabled={!!disabledReason}
              title={disabledReason ?? "Run this draft against the report's current content"}
            >
              <FaPlay /> Generate
            </Button>
            <Button
              variant="secondary"
              onClick={() => {
                setShowPlanned(!hasActual);
                setTab('transcript');
              }}
              title="See the requests this draft sends to the model"
            >
              <FaCode /> View prompt
            </Button>
            {isPending && (
              <span className="status" role="status">
                <Spinner size="1rem" /> Generating in the reporting service…
              </span>
            )}
            {test?.status === 'Completed' && (
              <span className="status" role="status">
                Completed
              </span>
            )}
            {test?.status === 'Failed' && (
              <span className="status failed" role="status">
                Failed{test.error ? `: ${test.error}` : ''}
              </span>
            )}
          </div>
          {test && test.status !== 'Pending' && (
            <div className="usage">
              {test.requestCount} request{test.requestCount === 1 ? '' : 's'} · {test.storyCount}{' '}
              stor{test.storyCount === 1 ? 'y' : 'ies'} · {test.promptTokens.toLocaleString()}{' '}
              prompt tokens · {test.completionTokens.toLocaleString()} completion tokens ·{' '}
              {(test.durationMs / 1000).toFixed(1)} s
            </div>
          )}
          <p className="hint">
            The draft is not saved until you apply it to the section and save the report. A test run
            is kept only until you leave this page and never affects the preview or sent reports.
          </p>
        </div>
        <div className="results">
          <div className="tabs" role="tablist">
            <button
              type="button"
              role="tab"
              aria-selected={tab === 'output'}
              onClick={() => setTab('output')}
            >
              Output
            </button>
            <button
              type="button"
              role="tab"
              aria-selected={tab === 'transcript'}
              onClick={() => setTab('transcript')}
            >
              Transcript
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
            {tab === 'output' &&
              (test?.status === 'Completed' ? (
                <div className="output">
                  <ReportBody html={test.output ?? ''} />
                </div>
              ) : test?.status === 'Failed' ? (
                <p role="alert">{test.error ?? 'The test run failed.'}</p>
              ) : isPending ? (
                <p>The section is being generated. The output appears here when it is ready.</p>
              ) : (
                <p>Generate the draft to see its output here.</p>
              ))}
            {tab === 'transcript' && (
              <>
                {hasActual && (
                  <p className="hint">
                    <button type="button" onClick={() => setShowPlanned(!showPlanned)}>
                      {showPlanned ? 'Show the last run' : 'Show the planned prompt for the draft'}
                    </button>
                  </p>
                )}
                {hasActual && !showPlanned ? (
                  <AIPromptTranscript exchanges={test!.transcript} />
                ) : (
                  <AIPromptTranscript
                    exchanges={plan?.exchanges ?? []}
                    isPlanned
                    isLoading={isPlanning || (planFor !== draftKey && !plan)}
                    error={plan?.error}
                  />
                )}
              </>
            )}
            {tab === 'chat' && (
              <>
                <p className="hint">
                  Ask how to change the prompt. The model sees the draft, the pipeline rules and the
                  last test run{test ? '' : ' (none yet)'}.
                </p>
                <div className="thread">
                  {thread.map((turn, i) => (
                    <div key={i} className={`turn ${turn.role}`}>
                      {turn.text}
                      {turn.suggestedPrompt && (
                        <div className="suggested">
                          <div dangerouslySetInnerHTML={{ __html: turn.suggestedPrompt }} />
                          <Button
                            variant="secondary"
                            onClick={() => setDraftPrompt(turn.suggestedPrompt ?? '')}
                          >
                            Use this prompt
                          </Button>
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
                <div className="ask">
                  <textarea
                    aria-label="Question"
                    placeholder="How should I change the prompt to…"
                    value={question}
                    onChange={(e) => setQuestion(e.target.value)}
                    onKeyDown={(e) => {
                      if (e.key === 'Enter' && (e.ctrlKey || e.metaKey)) ask();
                    }}
                  />
                  <Button
                    variant="primary"
                    onClick={ask}
                    disabled={isAsking || !question.trim() || !reportId || !sectionId}
                    title={
                      !reportId || !sectionId
                        ? 'Save the report to chat about this section'
                        : 'Send'
                    }
                  >
                    <FaPaperPlane /> Send
                  </Button>
                </div>
              </>
            )}
          </div>
        </div>
      </Workbench>
    </WorkbenchDialog>
  );
};
