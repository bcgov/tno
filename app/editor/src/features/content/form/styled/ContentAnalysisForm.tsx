import styled from 'styled-components';

export const ContentAnalysisForm = styled.div`
  margin-top: 1rem;
  min-width: 0;
  max-width: 100%;
  overflow-x: hidden;
  // Long unbroken text (an LLM error, a URL) wraps rather than widening the tab.
  overflow-wrap: anywhere;

  .analysis-status {
    margin-bottom: 0.5rem;
    flex-wrap: nowrap;

    & > div:first-child {
      min-width: 0;
    }

    .run-status {
      display: block;
      font-style: italic;
    }
  }

  .analysis-runs {
    margin-bottom: 0.5rem;
    font-size: 0.9rem;
  }

  // Only the results scroll, within the height of the Summary tab's editor, so the form's buttons
  // below stay in view.
  .analysis-body {
    max-height: 400px;
    overflow-y: auto;
    overflow-x: hidden;
  }

  h3 {
    font-size: 1rem;
    margin: 0 0 0.25rem 0;
  }

  ul {
    margin: 0;
  }
`;
