import { useFormikContext } from 'formik';
import moment from 'moment';
import React from 'react';
import { toast } from 'react-toastify';
import { useContentAnalysis } from 'store/hooks';
import { Button, ButtonVariant, Col, Row, Show } from 'tno-core';

import { type IContentAnalysisDetailsModel, type IContentForm } from './interfaces';
import * as styled from './styled';

const fieldNames: Record<string, string> = {
  summary: 'Summary',
  contributor: 'Contributor',
  tag: 'Tag',
  quote: 'Quote',
  topic: 'Topic',
};

/**
 * Shows a story's Content-Analysis result: its summary, facts, people and places, topics, quotes,
 * and which editorial values analysis populated. Analysis never changes a value a person set.
 * @returns Component.
 */
export const ContentAnalysisForm: React.FC = () => {
  const { values } = useFormikContext<IContentForm>();
  const { getContentAnalysis, requestAnalysis } = useContentAnalysis();
  const [details, setDetails] = React.useState<IContentAnalysisDetailsModel>();

  const load = React.useCallback(async () => {
    if (!values.id) return;
    try {
      setDetails(await getContentAnalysis(values.id));
    } catch {}
  }, [getContentAnalysis, values.id]);

  React.useEffect(() => {
    load();
    // Reload when the story's version changes (e.g. analysis populated it).
  }, [load, values.version]);

  const handleRequest = async () => {
    try {
      await requestAnalysis(values.id);
      toast.success('The story will be analyzed shortly.');
      await load();
    } catch {}
  };

  const analysis = details?.analysis;
  const populated = details?.ownership.filter((o) => o.owner === 'Analysis' && !o.isCleared) ?? [];
  const cleared = details?.ownership.filter((o) => o.isCleared) ?? [];

  return (
    <styled.ContentAnalysisForm>
      <Row gap="1rem" alignItems="center" className="analysis-status">
        <Col flex="1 1 auto">
          <Show visible={!!analysis}>
            <span>
              Analyzed {moment(analysis?.analyzedOn).format('YYYY-MM-DD HH:mm')} by{' '}
              {analysis?.model}
              {analysis?.isMetadataOnly ? ' (no usable text; metadata only)' : ''}
            </span>
          </Show>
          <Show visible={!analysis}>
            <span>This story has not been analyzed.</span>
          </Show>
          <Show visible={!!details?.job && details.job.status !== 'Completed'}>
            <span className="job-status">
              Analysis {details?.job?.status.toLowerCase()}
              {details?.job?.lastError ? `: ${details.job.lastError}` : ''}
            </span>
          </Show>
        </Col>
        <Button variant={ButtonVariant.secondary} onClick={handleRequest} disabled={!values.id}>
          Analyze again
        </Button>
      </Row>
      <Show visible={!!analysis}>
        <Col gap="0.75rem" direction="column" nowrap className="analysis-body">
          <Show visible={!!analysis?.summary}>
            <section>
              <h3>Summary</h3>
              <p>{analysis?.summary}</p>
            </section>
          </Show>
          <Show visible={!!analysis?.keyFacts.length}>
            <section>
              <h3>Key facts</h3>
              <ul>
                {analysis?.keyFacts.map((fact, i) => (
                  <li key={i}>
                    {fact.statement}
                    {fact.isInferred ? <em> (inferred)</em> : null}
                  </li>
                ))}
              </ul>
            </section>
          </Show>
          <Show visible={!!analysis?.topics.length}>
            <section>
              <h3>Topics</h3>
              <p>
                {analysis?.topics.map((t) => t.label).join(', ')}
                {analysis?.staffTopic ? ` — matches topic "${analysis.staffTopic}"` : ''}
              </p>
            </section>
          </Show>
          <Show visible={!!analysis?.entities.length}>
            <section>
              <h3>People and organizations</h3>
              <ul>
                {analysis?.entities.map((e, i) => (
                  <li key={i}>
                    {e.name}
                    {e.roles?.length ? ` — ${e.roles.join(', ')}` : ''}
                    {e.isAmbiguous ? <em> (ambiguous)</em> : null}
                  </li>
                ))}
              </ul>
            </section>
          </Show>
          <Show visible={!!analysis?.places.length}>
            <section>
              <h3>Places</h3>
              <p>
                {analysis?.places
                  .map((p) => (p.role ? `${p.name} (${p.role})` : p.name))
                  .join(', ')}
              </p>
            </section>
          </Show>
          <Show visible={!!analysis?.events.length}>
            <section>
              <h3>Reported events</h3>
              <ul>
                {analysis?.events.map((e, i) => (
                  <li key={i}>
                    {e.actor} {e.action}
                    {e.date ? `, ${e.date}` : ''}
                    {e.location ? `, ${e.location}` : ''}
                  </li>
                ))}
              </ul>
            </section>
          </Show>
          <Show visible={!!analysis?.quotes.length}>
            <section>
              <h3>Quotes found</h3>
              <ul>
                {analysis?.quotes.map((q, i) => (
                  <li key={i}>
                    “{q.statement}”{q.speaker ? ` — ${q.speaker}` : ''}
                  </li>
                ))}
              </ul>
            </section>
          </Show>
          <Show visible={!!populated.length || !!cleared.length}>
            <section>
              <h3>Fields</h3>
              <ul>
                {populated.map((o) => (
                  <li key={`${o.field}-${o.valueKey}`}>
                    {fieldNames[o.field] ?? o.field} set by analysis
                  </li>
                ))}
                {cleared.map((o) => (
                  <li key={`cleared-${o.field}-${o.valueKey}`}>
                    {fieldNames[o.field] ?? o.field} removed by an editor; analysis will not refill
                    it
                  </li>
                ))}
              </ul>
            </section>
          </Show>
        </Col>
      </Show>
    </styled.ContentAnalysisForm>
  );
};
