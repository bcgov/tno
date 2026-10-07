import moment from 'moment';
import React from 'react';
import { useContentAnalysis } from 'store/hooks/subscriber/useContentAnalysis';
import { Col, Show } from 'tno-core';

import { IContentAnalysisModel } from './IContentAnalysisModel';

export const ContentAnalysis: React.FC<{ contentId: number }> = ({ contentId }) => {
  const getAnalysis = useContentAnalysis();
  const [analysis, setAnalysis] = React.useState<IContentAnalysisModel | null>();
  const [failed, setFailed] = React.useState(false);
  React.useEffect(() => {
    let cancelled = false;
    setAnalysis(undefined);
    setFailed(false);
    getAnalysis(contentId)
      .then((result) => {
        if (!cancelled) setAnalysis(result ?? null);
      })
      .catch(() => {
        if (!cancelled) setFailed(true);
      });
    return () => {
      cancelled = true;
    };
  }, [contentId, getAnalysis]);

  return (
    <div aria-live="polite">
      <h2>Analysis</h2>
      <Show visible={failed}>
        <p>Unable to load analysis. Please reopen this panel to try again.</p>
      </Show>
      <Show visible={!failed && analysis === undefined}>
        <p>Loading analysis...</p>
      </Show>
      <Show visible={analysis === null}>
        <p>This story has not been analyzed.</p>
      </Show>
      <Show visible={!!analysis}>
        <p>
          Analyzed {moment(analysis?.analyzedOn).format('YYYY-MM-DD HH:mm')} by {analysis?.model}
          {analysis?.isMetadataOnly ? ' (no usable text; metadata only)' : ''}
        </p>
      </Show>
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
        </Col>
      </Show>
    </div>
  );
};
