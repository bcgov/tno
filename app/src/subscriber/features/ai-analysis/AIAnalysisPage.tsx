import { Button } from 'components/button';
import { PageSection } from 'components/section';
import React from 'react';
import { useAI } from 'store/hooks/subscriber';
import { BouncingSpinner, Show, Wysiwyg } from 'tno-core';

import * as styled from './styled';

export const AIAnalysisPage: React.FC = () => {
  const { analyze } = useAI();
  const [prompt, setPrompt] = React.useState('');
  const [response, setResponse] = React.useState('');
  const [loading, setLoading] = React.useState(false);

  const handleSubmit = async () => {
    if (!prompt.trim()) return;
    try {
      setLoading(true);
      setResponse('');
      const result = await analyze(prompt);
      setResponse(result);
    } catch {
      // Error toast is handled by useAjaxWrapper → useToastError in DefaultLayout.
    } finally {
      setLoading(false);
    }
  };

  return (
    <styled.AIAnalysisPage>
      <PageSection header="AI Analysis" includeHeaderIcon>
        <div className="two-col">
          <div className="prompt-col">
            <div className="col-label">Prompt</div>
            <Wysiwyg value={prompt} onChange={setPrompt} placeholder="Enter your prompt here..." />
            <div className="prompt-actions">
              <Button onClick={handleSubmit} disabled={loading || !prompt.trim()}>
                {loading ? 'Analyzing...' : 'Submit'}
              </Button>
            </div>
          </div>
          <div className="response-col">
            <div className="col-label">Response</div>
            <Show visible={loading}>
              <div className="response-panel loading">
                <BouncingSpinner />
                <span>Waiting for response...</span>
              </div>
            </Show>
            <Show visible={!loading && !!response}>
              <div className="response-panel" dangerouslySetInnerHTML={{ __html: response }} />
            </Show>
            <Show visible={!loading && !response}>
              <div className="response-panel empty">Response will appear here.</div>
            </Show>
          </div>
        </div>
      </PageSection>
    </styled.AIAnalysisPage>
  );
};
