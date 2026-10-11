import React from 'react';
import { FaCopy, FaDownload } from 'react-icons/fa6';
import { toast } from 'react-toastify';
import { ILlmExchangeModel, ILlmMessageModel } from 'store/hooks/subscriber/interfaces';
import styled from 'styled-components';

const Transcript = styled.div`
  display: flex;
  flex-direction: column;
  gap: 0.75rem;

  .transcript-toolbar {
    display: flex;
    flex-wrap: wrap;
    align-items: center;
    gap: 0.5rem;

    .transcript-note {
      flex: 1 1 20rem;
      margin: 0;
      padding: 0.4rem 0.6rem;
      border-left: 4px solid #8a7fb8;
      background: #f3f0fa;
      font-size: 0.9rem;
    }
  }

  button {
    display: inline-flex;
    align-items: center;
    gap: 0.35rem;
    padding: 0.25rem 0.6rem;
    background: #fff;
    border: 1px solid #bbb;
    border-radius: 0.25rem;
    font-size: 0.85rem;
    cursor: pointer;
    white-space: nowrap;

    &:hover {
      background: #f5f5f5;
    }
  }

  details {
    border: 1px solid #ddd;
    border-radius: 0.35rem;

    > summary {
      display: flex;
      flex-wrap: wrap;
      align-items: center;
      gap: 0.75rem;
      padding: 0.5rem 0.75rem;
      cursor: pointer;
      background: #f7f7f7;
      font-weight: 600;

      .stage-meta {
        font-weight: 400;
        font-size: 0.85rem;
        color: #555;
      }

      .stage-error {
        color: #b00020;
        font-weight: 400;
        font-size: 0.85rem;
      }

      button {
        margin-left: auto;
      }
    }
  }

  .message {
    padding: 0.5rem 0.75rem;
    border-top: 1px solid #eee;

    .message-header {
      display: flex;
      align-items: center;
      gap: 0.5rem;
      margin-bottom: 0.25rem;

      .role {
        font-family: monospace;
        font-size: 0.85rem;
        padding: 0.1rem 0.4rem;
        border-radius: 0.25rem;
        background: #e8e8e8;
      }

      .role-response {
        background: #dcefdc;
      }

      .role-error {
        background: #f8d7da;
      }

      button {
        margin-left: auto;
      }
    }

    pre {
      margin: 0;
      padding: 0.6rem;
      max-height: 24rem;
      overflow: auto;
      white-space: pre-wrap;
      overflow-wrap: anywhere;
      background: #fafafa;
      border: 1px solid #eee;
      font-size: 0.85rem;
    }
  }
`;

export interface IAIPromptTranscriptProps {
  /** The exchanges, in any order; shown by sequence. */
  exchanges: ILlmExchangeModel[];
  /** True when the exchanges were planned rather than sent: no responses, batches may still split. */
  isPlanned?: boolean;
  /** Why the transcript could not be produced. */
  error?: string;
  /** Shown while the exchanges are being fetched. */
  isLoading?: boolean;
  /** The file name used when the transcript is downloaded. */
  fileName?: string;
}

const copyText = async (text: string, what: string) => {
  try {
    await navigator.clipboard.writeText(text);
    toast.success(`${what} copied`);
  } catch {
    toast.error('The clipboard is not available. Select the text and copy it instead.');
  }
};

const roleLabel = (message: ILlmMessageModel) => message.role || 'user';

/** The exchanges as plain text, for copying or downloading. */
export const formatTranscript = (exchanges: ILlmExchangeModel[]) =>
  [...exchanges]
    .sort((a, b) => a.sequence - b.sequence)
    .map(formatExchange)
    .join('\n\n');

/** One exchange as plain text. */
export const formatExchange = (exchange: ILlmExchangeModel) => {
  const parts = [`=== ${exchange.stage} ===`];
  exchange.messages.forEach((m) => parts.push(`--- ${roleLabel(m)} ---\n${m.content}`));
  if (exchange.response != null) parts.push(`--- response ---\n${exchange.response}`);
  if (exchange.error) parts.push(`--- error ---\n${exchange.error}`);
  return parts.join('\n');
};

const describe = (exchange: ILlmExchangeModel, isPlanned: boolean) => {
  const parts: string[] = [];
  if (isPlanned) parts.push(`~${exchange.estimatedTokens.toLocaleString()} tokens`);
  else {
    if (exchange.promptTokens != null)
      parts.push(`${exchange.promptTokens.toLocaleString()} prompt tokens`);
    if (exchange.completionTokens != null)
      parts.push(`${exchange.completionTokens.toLocaleString()} completion tokens`);
    if (exchange.durationMs) parts.push(`${(exchange.durationMs / 1000).toFixed(1)} s`);
  }
  if (exchange.jsonMode) parts.push('JSON');
  if (exchange.isTruncated) parts.push('truncated');
  return parts.join(' · ');
};

/**
 * Shows every request an AI section sends (or would send) to its model, grouped by stage with
 * copy and download actions.
 */
export const AIPromptTranscript: React.FC<IAIPromptTranscriptProps> = ({
  exchanges,
  isPlanned = false,
  error,
  isLoading,
  fileName = 'ai-section-prompt.txt',
}) => {
  const ordered = React.useMemo(
    () => [...exchanges].sort((a, b) => a.sequence - b.sequence),
    [exchanges],
  );

  const download = () => {
    const blob = new Blob([formatTranscript(ordered)], { type: 'text/plain;charset=utf-8' });
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = fileName;
    link.click();
    URL.revokeObjectURL(url);
  };

  if (isLoading) return <p>Loading the prompt…</p>;
  if (error) return <p role="alert">{error}</p>;
  if (!ordered.length) return <p>Nothing has been sent to the model yet.</p>;

  return (
    <Transcript className="ai-prompt-transcript">
      <div className="transcript-toolbar">
        {isPlanned ? (
          <p className="transcript-note">
            Planned prompt: what will be sent for the report&apos;s current content. Batches may be
            split at run time; findings and responses appear after a run.
          </p>
        ) : (
          <p className="transcript-note">
            Actual prompt: every request sent during the run, in order, with the model&apos;s
            responses.
          </p>
        )}
        <button type="button" onClick={() => copyText(formatTranscript(ordered), 'Transcript')}>
          <FaCopy /> Copy all
        </button>
        <button type="button" onClick={download}>
          <FaDownload /> Download .txt
        </button>
      </div>
      {ordered.map((exchange) => (
        <details key={exchange.sequence} open={exchange.kind === 'Final' || ordered.length <= 3}>
          <summary>
            <span>{exchange.stage}</span>
            <span className="stage-meta">{describe(exchange, isPlanned)}</span>
            {exchange.error && <span className="stage-error">{exchange.error}</span>}
            <button
              type="button"
              onClick={(e) => {
                e.preventDefault();
                copyText(formatExchange(exchange), exchange.stage);
              }}
            >
              <FaCopy /> Copy stage
            </button>
          </summary>
          {exchange.messages.map((message, i) => (
            <div key={i} className="message">
              <div className="message-header">
                <span className="role">{roleLabel(message)}</span>
                <button
                  type="button"
                  onClick={() => copyText(message.content, `${roleLabel(message)} message`)}
                >
                  <FaCopy /> Copy
                </button>
              </div>
              <pre>{message.content}</pre>
            </div>
          ))}
          {exchange.response != null && (
            <div className="message">
              <div className="message-header">
                <span className="role role-response">response</span>
                <button type="button" onClick={() => copyText(exchange.response ?? '', 'Response')}>
                  <FaCopy /> Copy
                </button>
              </div>
              <pre>{exchange.response}</pre>
            </div>
          )}
          {exchange.error && (
            <div className="message">
              <div className="message-header">
                <span className="role role-error">error</span>
              </div>
              <pre>{exchange.error}</pre>
            </div>
          )}
        </details>
      ))}
    </Transcript>
  );
};
