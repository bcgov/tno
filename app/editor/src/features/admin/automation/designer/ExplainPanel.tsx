import React from 'react';
import { toast } from 'react-toastify';
import {
  Button,
  ButtonVariant,
  Col,
  type IOptionItem,
  Row,
  Select,
  Show,
  TextArea,
} from 'tno-core';

import { createOption, findOptionByValue } from '../utils';
import {
  type IAutomationExplainRequestModel,
  type IAutomationExplainResultModel,
} from './interfaces';

export interface IExplainPanelProps {
  runId: number;
  logId: number;
  onExplain: (
    runId: number,
    logId: number,
    request: IAutomationExplainRequestModel,
  ) => Promise<IAutomationExplainResultModel>;
  /** Prompt library entry names a suggested revision can be applied to. */
  promptNames?: string[];
  /** Apply a suggested revision to a prompt library entry (an explicit admin action). */
  onApplyPrompt?: (name: string, text: string) => void;
}

interface IExchange {
  question: string;
  answer: string;
  suggestedPrompt?: string | null;
}

/**
 * Explain-and-improve conversation about one log entry. The first turn is seeded server-side with
 * the entry's exact prompt, response, parsed outcome, and configuration. Suggested prompt
 * revisions are shown for review — nothing is ever applied without an explicit admin action here.
 */
export const ExplainPanel: React.FC<IExplainPanelProps> = ({
  runId,
  logId,
  onExplain,
  promptNames = [],
  onApplyPrompt,
}) => {
  const [question, setQuestion] = React.useState('');
  const [exchanges, setExchanges] = React.useState<IExchange[]>([]);
  const [messages, setMessages] = React.useState<{ role: string; content: string }[]>([]);
  const [asking, setAsking] = React.useState(false);
  const [applyTarget, setApplyTarget] = React.useState('');

  const promptOptions = promptNames.map((name) => createOption(name, name));

  const ask = async () => {
    const text = question.trim();
    if (!text) return;
    setAsking(true);
    try {
      const result = await onExplain(runId, logId, { question: text, messages });
      setExchanges((current) => [
        ...current,
        { question: text, answer: result.answer, suggestedPrompt: result.suggestedPrompt },
      ]);
      setMessages(result.messages);
      setQuestion('');
    } catch {
      toast.error('The explain request failed.');
    } finally {
      setAsking(false);
    }
  };

  return (
    <Col className="automation-explain-panel" gap="0.5rem">
      <p className="automation-field-help">
        Ask why this decision was made or how to improve the prompt. The assistant reasons about the
        recorded exchange — it does not re-run it — and its suggestions are proposals until you
        apply and save them.
      </p>
      {exchanges.map((exchange, index) => (
        <Col key={index} className="automation-explain-exchange" gap="0.25rem">
          <div className="automation-explain-question">
            <label>You:</label> {exchange.question}
          </div>
          <div className="automation-explain-answer">
            <label>Assistant:</label>
            <pre>{exchange.answer}</pre>
          </div>
          <Show visible={!!exchange.suggestedPrompt}>
            <Col className="automation-explain-suggestion" gap="0.25rem">
              <label>Proposed prompt revision:</label>
              <pre>{exchange.suggestedPrompt}</pre>
              <Row gap="0.5rem" alignItems="flex-end" nowrap>
                <Button
                  variant={ButtonVariant.secondary}
                  onClick={() => {
                    navigator.clipboard.writeText(exchange.suggestedPrompt ?? '');
                    toast.success('Revision copied to the clipboard.');
                  }}
                >
                  Copy
                </Button>
                <Show visible={!!onApplyPrompt && promptOptions.length > 0}>
                  <Select
                    name={`explain-apply-target-${index}`}
                    width="16rem"
                    placeholder="prompt library entry…"
                    options={promptOptions}
                    value={findOptionByValue(promptOptions, applyTarget) ?? ''}
                    onChange={(newValue) => {
                      const option = newValue as IOptionItem;
                      setApplyTarget(option?.value ? `${option.value}` : '');
                    }}
                  />
                  <Button
                    variant={ButtonVariant.secondary}
                    disabled={!applyTarget}
                    tooltip="Replaces the library entry in the unsaved profile; review and Save to keep it."
                    onClick={() => {
                      onApplyPrompt!(applyTarget, exchange.suggestedPrompt ?? '');
                      toast.success(
                        `Revision applied to '${applyTarget}' in the unsaved profile - review and Save to keep it.`,
                      );
                    }}
                  >
                    Apply to prompt
                  </Button>
                </Show>
              </Row>
            </Col>
          </Show>
        </Col>
      ))}
      <TextArea
        name="explain-question"
        rows={2}
        placeholder="e.g. Why was this not confirmed? How should the prompt change to catch this case?"
        value={question}
        onChange={(e) => setQuestion(e.target.value)}
      />
      <Row>
        <Button disabled={asking || !question.trim()} onClick={ask}>
          {asking ? 'Asking…' : 'Ask'}
        </Button>
      </Row>
    </Col>
  );
};
